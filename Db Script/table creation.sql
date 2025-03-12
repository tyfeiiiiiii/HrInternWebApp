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
EmpId INT PRIMARY KEY,
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
lastUpdated DATETIME,
CONSTRAINT FK_LeaveBalance_Employee FOREIGN KEY (empId) REFERENCES Employee (empId)
)

