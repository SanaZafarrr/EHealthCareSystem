CREATE TABLE ChatLog (
    Id INT IDENTITY(1,1) PRIMARY KEY,
    PatientId INT,
    MessageText NVARCHAR(MAX),
    PredictedDisease NVARCHAR(100),
    AIResponse NVARCHAR(MAX),
    SuggestedSpecialization NVARCHAR(100),
    Urgency NVARCHAR(20),
    CreatedAt DATETIME DEFAULT GETDATE(),
    SessionId NVARCHAR(50)
);