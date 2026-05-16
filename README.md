# E-Health Care: A Strategic Management System with AI-Driven Chatbot

## Overview
A web-based healthcare management system built with ASP.NET Core 8 MVC with
an AI-powered medical chatbot using TinyLlama (LoRA fine-tuned on
ChatDoctor-HealthCareMagic-100k) deployed via Ollama.

## Actors & Features
- Admin: Add, manage doctors and patients, profile management
- Doctor: Manage appointments, add prescriptions, view patient records
- Patient Book/cancel appointments, view history, AI health chatbot

## AI Chatbot
| Property | Details |
|----------|---------|
| Model | TinyLlama |
| Dataset | ChatDoctor-HealthCareMagic-100k |
| Fine-Tuning | LoRA on Google Colab (T4 GPU) |
| Deployment | Ollama (localhost:11434) |

## Tech Stack
ASP.NET Core 8 MVC · SQL Server · Entity Framework Core · Bootstrap 5 · Ollama

## Installation
1. Clone the repo
2. Update connection string in `appsettings.json`
3. Run `Update-Database` in Package Manager Console
4. Run `ollama pull phi3:mini` in CMD
5. Press **F5** in Visual Studio

## Team
| Name | Roll Number |
|------|-------------|
| Sana Zafar | BITF22M506 |
| Bitia Mushtaq | BITF22M543|
| Ifrah Shakeel | BITF22M540|

## Supervisor
Dr. Nadeem Akhtar — PUCIT, Lahore