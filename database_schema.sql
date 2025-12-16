-- Database Schema for Billing Automation

-- 1. Customer Master
CREATE TABLE CustomerMaster (
    CustomerID NVARCHAR(50) PRIMARY KEY,
    Plan NVARCHAR(50),
    CityRegion NVARCHAR(50),
    CreatedDate DATETIME DEFAULT CURRENT_TIMESTAMP
);

-- 2. Customer Usage
CREATE TABLE CustomerUsage (
    UsageID INTEGER PRIMARY KEY AUTOINCREMENT, -- or IDENTITY(1,1) for SQL Server
    CustomerID NVARCHAR(50),
    TotalUsage INT,
    CalculatedBillAmount DECIMAL(18, 2),
    ProcessedDate DATETIME DEFAULT CURRENT_TIMESTAMP,
    FOREIGN KEY (CustomerID) REFERENCES CustomerMaster(CustomerID)
);

-- 3. Batch Analysis Log (Stores AI Summaries)
CREATE TABLE BatchAnalysisLog (
    Id NVARCHAR(50) PRIMARY KEY,
    ProcessedAt DATETIME DEFAULT CURRENT_TIMESTAMP,
    AiAnalysis NVARCHAR(MAX) -- Stores JSON structure
);
