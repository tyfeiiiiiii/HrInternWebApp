USE [HRManagementSystem];

CREATE TABLE Employee (
    empId INT NOT NULL,
    username VARCHAR(50),
    password VARCHAR(50),

    CONSTRAINT PK_Employees PRIMARY KEY (empId)
);

ALTER TABLE Employee ADD Role VARCHAR(50);
ALTER TABLE Employee ADD Department VARCHAR(50);
ALTER TABLE Employee ADD email VARCHAR(50);
ALTER TABLE Employee ADD profilePic VARBINARY(MAX);
ALTER TABLE Employee ADD Gender NVARCHAR(10);

CREATE TABLE Leave (
    leaveId INT IDENTITY(1,1) PRIMARY KEY,
    empId INT,
    leaveType NVARCHAR(50),
    startDate DATETIME,
    endDate DATETIME,
    reason NVARCHAR(255),
    status NVARCHAR(50),
    approver NVARCHAR(50)
);
ALTER TABLE Leave
ADD CONSTRAINT FK_Leave_Employee
FOREIGN KEY (empId)
REFERENCES Employee(empId);



CREATE TABLE LeaveBalance(
LeaveBalanceId INT IDENTITY(1,1) PRIMARY KEY,
EmpId INT NOT NULL,
medicalLeave INT,
annualLeave INT,
hospitalization INT,
examination INT,
marriage INT,
paternityLeave INT,
maternityLeave INT,
childcareLeave INT,
unpaidLeave INT,
emergencyLeave INT,
lastUpdated DATETIME DEFAULT CURRENT_TIMESTAMP,
CONSTRAINT FK_LeaveBalance_Employee FOREIGN KEY (EmpId) REFERENCES Employee(empId)
)

ALTER TABLE LeaveBalance 
ADD MedicalLeaveUsed INT DEFAULT 0,
    AnnualLeaveUsed INT DEFAULT 0,
    HospitalizationUsed INT DEFAULT 0,
    ExaminationUsed INT DEFAULT 0,
    MarriageUsed INT DEFAULT 0,
    PaternityLeaveUsed INT DEFAULT 0,
    MaternityLeaveUsed INT DEFAULT 0,
    ChildcareLeaveUsed INT DEFAULT 0,
    UnpaidLeaveUsed INT DEFAULT 0,
    EmergencyLeaveUsed INT DEFAULT 0;


CREATE TABLE Survey (
    SurveyId INT IDENTITY(1,1) PRIMARY KEY,
    EmployeeId INT NOT NULL,
    SatisfactionLevel FLOAT NOT NULL,
    LastEvaluation FLOAT NOT NULL,
    NumberProject INT NOT NULL,
    AverageMonthlyHours INT NOT NULL,
    TimeSpendCompany INT NOT NULL,
    WorkAccident BIT NOT NULL,
    PromotionLast5Years BIT NOT NULL,
    Department NVARCHAR(100) NOT NULL,
    Salary NVARCHAR(50) NOT NULL,
    SubmissionDate DATETIME NOT NULL,
    CONSTRAINT FK_Survey_Employee FOREIGN KEY (EmployeeId) REFERENCES Employee(empId)
);

