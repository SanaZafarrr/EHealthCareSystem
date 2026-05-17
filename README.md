# E-Health Care: A Strategic Management System with AI-Driven Chatbot


## Overview
A web-based healthcare management system built with ASP.NET Core 8 MVC, integrating an AI-powered medical chatbot using TinyLlama (LoRA fine-tuned on ChatDoctor-HealthCareMagic-100k) deployed locally via Ollama. Supports three roles: Admin, Doctor, and Patient.

---

## Technology Stack

| Layer         | Technology                           |
|---------------|--------------------------------------|
| Frontend      | Razor Views, Bootstrap 5, HTML, CSS  |
| Backend       | C#, ASP.NET Core 8 MVC               |
| Database      | Microsoft SQL Server, EF Core        |
| AI Model      | TinyLlama (LoRA Fine-Tuned)          |
| AI Dataset    | ChatDoctor-HealthCareMagic-100k      |
| Fine-Tuning   | LoRA on Google Colab (T4 GPU)        |
| AI Deployment | Ollama (localhost:11434)             |
| IDE           | Visual Studio 2022                   |

---

## Actors and Features

| Actor   | Features                                                                 |
|---------|--------------------------------------------------------------------------|
| Admin   | Login, manage profile, add/view/update/delete doctors and patients       |
| Doctor  | Login, manage appointments, add prescriptions, view patient records      |
| Patient | Register, book/cancel appointments, view history, AI health chatbot      |

---

## AI Chatbot

| Property         | Details                          |
|------------------|----------------------------------|
| Foundation Model | TinyLlama (1.1B parameters)      |
| Dataset          | ChatDoctor-HealthCareMagic-100k  |
| Fine-Tuning      | LoRA on Google Colab (T4 GPU)    |
| Deployment       | Ollama — free, offline, local    |
| Task             | Preliminary Healthcare Guidance  |
| Access           | Patient module only              |

---

## Project Structure

```
HealthCareSystem/
├── Controllers/        # Admin, Doctor, Patient, ChatAI, Account
├── Models/             # User, Doctor, Patient, Appointment, Prescription
├── Views/
│   ├── Admin/
│   ├── Doctor/
│   └── Patient/
│       └── ChatAI.cshtml
├── Services/
│   └── OllamaChatService.cs
├── Data/
│   └── AppDbContext.cs
├── wwwroot/
├── appsettings.json
└── Program.cs
```

---

## Installation

### Prerequisites
- Visual Studio 2022, .NET 8 SDK, SQL Server, Ollama (https://ollama.com)

### Steps

```bash
# 1. Clone the repository
git clone https://github.com/SanaZafarrr/EHealthCareSystem.git

# 2. Update connection string in appsettings.json
"HospitalConnection": "Server=(localdb)\\MSSQLLocalDB;Database=ISBHospitalDB;Trusted_Connection=True;"

# 3. Apply migrations (Package Manager Console)
Update-Database

# 4. Pull AI model
ollama pull phi3:mini

# 5. Run the project
Press F5 in Visual Studio
```

---

## Default Credentials

| Role    | Email                   | Password   |
|---------|-------------------------|------------|
| Admin   | sanazafar1076@gmail.com | sana12345  |
| Doctor  | Added by Admin          |    --      |   
| Patient | Self-register           |    --      |

---

## Branches and Release

| Branch / Tag | Purpose                              |
|--------------|--------------------------------------|
| main         | Final production-ready code          |
| dev          | Feature integration and testing      |
| v1.0-final   | Final release — all modules complete |

---

## Academic Details

| Field      | Details                                          |
|------------|--------------------------------------------------|
| Project    | Final Year Project (FYP)                         |
| Degree     | BS Information Technology                        |
| Session    | Fall 2022 – 2026                                 |
| University | University of the Punjab, Lahore                 |

---

## Team Members

| Name          | Roll Number |
|---------------|-------------|
| Sana Zafar    | BITF22M506  |
| Bitia Mushtaq | BITF22M543  |
| Ifrah Shakeel | BITF22M540  |

---

## Supervisor
  
Dr. Nadeem Akhtar  
Faculty of Computing and Information Technology  
University of the Punjab, Lahore

---

## Disclaimer

This AI chatbot provides general health guidance only and is NOT a replacement for a qualified medical professional. Always consult a licensed doctor for medical advice.
