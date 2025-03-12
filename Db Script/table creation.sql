USE [HRManagementSystem];

-- Employee Table
CREATE TABLE Employee (
    empId INT IDENTITY(1,1) PRIMARY KEY,
    username VARCHAR(50) NOT NULL,
    password VARCHAR(50) NOT NULL,
    Role VARCHAR(50),
    Department VARCHAR(50),
    email VARCHAR(50) UNIQUE,
    profilePic VARBINARY(MAX),
    Gender NVARCHAR(10)
);

-- Leave Type Table (Defines all types of leave)
CREATE TABLE LeaveType (
    leaveTypeId INT IDENTITY(1,1) PRIMARY KEY,
    leaveTypeName NVARCHAR(50) NOT NULL UNIQUE
);

-- Leave Table (Tracks leave applications)
CREATE TABLE Leave (
    leaveId INT IDENTITY(1,1) PRIMARY KEY,
    empId INT NOT NULL,
    leaveTypeId INT NOT NULL,
    startDate DATETIME NOT NULL,
    endDate DATETIME NOT NULL,
    reason NVARCHAR(255),
    status NVARCHAR(50) DEFAULT 'Pending',
    approver NVARCHAR(50),
    FOREIGN KEY (empId) REFERENCES Employee(empId),
    FOREIGN KEY (leaveTypeId) REFERENCES LeaveType(leaveTypeId)
);

-- Leave Balance Table (Tracks leave balances per employee)
CREATE TABLE LeaveBalance (
    empId INT NOT NULL,
    leaveTypeId INT NOT NULL,
    balance INT NOT NULL,
    lastUpdated DATETIME DEFAULT GETDATE(),
    PRIMARY KEY (empId, leaveTypeId),
    FOREIGN KEY (empId) REFERENCES Employee(empId),
    FOREIGN KEY (leaveTypeId) REFERENCES LeaveType(leaveTypeId)
);

---- Stored Procedure: Assign default leave balance
--CREATE PROCEDURE AssignDefaultLeaveBalance 
--    @empId INT
--AS
--BEGIN
--    SET NOCOUNT ON;

--    INSERT INTO LeaveBalance (empId, leaveTypeId, balance)
--    SELECT @empId, leaveTypeId, 
--           CASE 
--               WHEN leaveTypeName = 'Annual Leave' THEN 15
--               WHEN leaveTypeName = 'Medical Leave' THEN 10
--               WHEN leaveTypeName = 'Paternity Leave' THEN 7
--               ELSE 0
--           END
--    FROM LeaveType;
--END;

---- Stored Procedure: Deduct Leave Balance
--CREATE PROCEDURE DeductLeaveBalance
--    @empId INT,
--    @leaveTypeId INT,
--    @daysTaken INT
--AS
--BEGIN
--    SET NOCOUNT ON;

--    UPDATE LeaveBalance
--    SET balance = balance - @daysTaken, lastUpdated = GETDATE()
--    WHERE empId = @empId AND leaveTypeId = @leaveTypeId;

--    -- Prevent negative balance
--    UPDATE LeaveBalance
--    SET balance = 0
--    WHERE empId = @empId AND leaveTypeId = @leaveTypeId AND balance < 0;
--END;
