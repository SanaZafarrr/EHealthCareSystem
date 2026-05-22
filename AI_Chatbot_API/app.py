from flask import Flask, request, jsonify
from flask_cors import CORS

import pandas as pd
import faiss
import numpy as np
import re
from sentence_transformers import SentenceTransformer
from difflib import get_close_matches

app = Flask(__name__)
CORS(app)

# =========================
# MODEL
# =========================
print("Loading embedding model...")
model = SentenceTransformer('all-MiniLM-L6-v2')
print("Model loaded!")

# =========================
# DATASET
# =========================
print("Loading MedQuAD dataset...")

df = pd.read_csv("medquad.csv")

q_cols = ["question", "input", "Question", "questions"]
a_cols = ["answer", "output", "Answer", "answerText", "answers"]

q_col = next((c for c in q_cols if c in df.columns), None)
a_col = next((c for c in a_cols if c in df.columns), None)

if not q_col or not a_col:
    raise ValueError("Invalid dataset format")

df = df[[q_col, a_col]].dropna()
df.columns = ["input", "output"]

df = df.head(15000)

# =========================
# CLEAN TEXT
# =========================
def clean_text(text):
    text = str(text).lower().strip()
    text = re.sub(r'[^a-z0-9\s]', ' ', text)
    text = re.sub(r'\s+', ' ', text)
    return text

df["input"] = df["input"].apply(clean_text)
df["output"] = df["output"].astype(str)

df = df.drop_duplicates(subset=["input"])
df = df[df["input"].str.len() > 5]
df = df[df["output"].str.len() > 20]

questions = df["input"].tolist()
answers = df["output"].tolist()

print("Dataset size:", len(questions))

# =========================
# QUERY EXPANSION (FIXED + STRONG)
# =========================
def expand_query(query):
    mapping = {
        "tired": "fatigue weakness anemia low energy iron deficiency",
        "fever": "high temperature infection flu viral body heat",
        "headache": "head pain migraine stress tension pressure",
        "cough": "dry cough throat infection respiratory",
        "breathing": "shortness of breath asthma lung disease dyspnea breathing difficulty",
        "stomach": "abdominal pain gastritis digestion infection stomach ache",
        "throat": "sore throat infection pain swallowing difficulty",
        "weak": "fatigue weakness low energy body weakness",
        "dizzy": "dizziness vertigo lightheaded fainting feeling",
        "pain": "ache discomfort soreness body pain"
    }

    for k, v in mapping.items():
        if k in query:
            query += " " + v

    return query

# =========================
# RESPONSE CLEANING
# =========================
def clean_response(text):
    text = str(text)

    text = re.sub(r'http\S+', '', text)

    remove_phrases = [
        "hospital admission",
        "emergency surgery",
        "prescribe",
        "start taking",
        "consult your doctor",
        "mayo clinic",
        "nih",
        "medlineplus"
    ]

    for p in remove_phrases:
        text = text.replace(p, "")

    sentences = text.split(".")
    return ". ".join(sentences[:2]).strip()

# =========================
# FILTERS
# =========================
def is_bad_response(text):
    text = text.lower()

    bad_keywords = [
        "ct scan", "mri", "endoscopy",
        "rare disease database",
        "genetic testing registry",
        "hospital admission"
    ]

    if len(text.split()) < 8:
        return True

    return any(b in text for b in bad_keywords)

def is_emergency(text):
    text = clean_text(text)
    keywords = [
        "chest pain", "difficulty breathing", "cannot breathe",
        "heart attack", "stroke", "unconscious",
        "suicide", "kill myself", "severe bleeding"
    ]
    return any(k in text for k in keywords)

def is_non_medical(text):
    text = clean_text(text)
    return any(i in text for i in ["hello", "hi", "who are you", "your name"])

def is_medicine_request(text):
    text = text.lower()
    return any(k in text for k in [
        "medicine", "drug", "tablet", "dose",
        "antibiotic", "prescribe", "painkiller"
    ])

# =========================
# EMBEDDINGS (FIXED + NORMALIZED)
# =========================
print("Creating embeddings...")

embeddings = model.encode(
    questions,
    convert_to_numpy=True,
    normalize_embeddings=True
).astype("float32")

index = faiss.IndexFlatIP(embeddings.shape[1])
index.add(embeddings)

print("FAISS ready!")

# =========================
# SEARCH ENGINE (FULL FIXED LOGIC)
# =========================
def search_medical(query):

    query = clean_text(query)

    # FIX 1: multi symptom handling (IMPORTANT)
    query = re.split(r"and|,|&", query)
    query = " ".join([q.strip() for q in query if len(q.strip()) > 1])

    query = expand_query(query)

    # FIX 2: keyword boost (improves recall)
    boost_words = ["fever", "cough", "headache", "pain", "flu", "throat", "weak", "dizzy"]
    for w in boost_words:
        if w in query:
            query += " " + w

    if len(query.split()) <= 2:
        match = get_close_matches(query, questions, n=1, cutoff=0.8)
        if match:
            query = match[0]

    q_vec = model.encode(
        [query],
        convert_to_numpy=True,
        normalize_embeddings=True
    ).astype("float32")

    D, I = index.search(q_vec, k=5)

    best_answer = None
    best_score = 0

    for idx, score in zip(I[0], D[0]):

        candidate = answers[idx]

        if is_bad_response(candidate):
            continue

        # safety boost filters
        if "breathing" in query and "breath" not in candidate.lower():
            continue

        if "throat" in query and "throat" not in candidate.lower():
            continue

        if score > best_score:
            best_score = score
            best_answer = candidate

    # FIX 3: SMART ADAPTIVE THRESHOLD (IMPORTANT FIX)
    avg_score = float(np.mean(D[0]))
    threshold = max(0.52, avg_score * 0.90)

    if best_answer and best_score >= threshold:
        return clean_response(best_answer)

    # FIX 4: fallback improved (no empty failure)
    return "We could not confidently match your symptoms. Please consult a doctor."

# =========================
# API
# =========================
@app.route("/chat", methods=["POST"])
def chat():

    try:
        data = request.get_json()
        msg = data.get("message", "").strip()

        if not msg:
            return jsonify({"reply": "Please enter symptoms", "is_emergency": False})

        if is_non_medical(msg):
            return jsonify({"reply": "Please describe your symptoms.", "is_emergency": False})

        if is_medicine_request(msg):
            return jsonify({
                "reply": "⚠️ Please do not take medicines without doctor's prescription.",
                "is_emergency": False
            })

        if is_emergency(msg):
            return jsonify({
                "reply": "🚨 Emergency detected. Call 1122 immediately.",
                "is_emergency": True
            })

        result = search_medical(msg)

        return jsonify({
            "reply": f"""
Advice:
{result}

Doctor Type:
General Physician

Warning:
Consult a doctor for proper diagnosis.
""",
            "is_emergency": False
        })

    except Exception as e:
        print("ERROR:", e)
        return jsonify({"reply": "Server error", "is_emergency": False})

# =========================
# RUN
# =========================
if __name__ == "__main__":
    print("Server running...")
    app.run(host="0.0.0.0", port=5000, debug=False)