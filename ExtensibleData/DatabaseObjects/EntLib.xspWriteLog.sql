CREATE PROCEDURE EntLib.xspWriteLog
	@EventID INT
	,@Priority INT
	,@Severity NVARCHAR(32)
	,@Title NVARCHAR(256)
	,@Timestamp DATETIME
	,@MachineName NVARCHAR(32)
	,@AppDomainName NVARCHAR(512)
	,@ProcessID NVARCHAR(256)
	,@ProcessName NVARCHAR(512)
	,@ThreadName NVARCHAR(512)
	,@Win32ThreadId NVARCHAR(128)
	,@Message NVARCHAR(1500)
	,@FormattedMessage NVARCHAR(MAX)
	,@LogId INT OUTPUT
AS 

	DECLARE @UserName NVARCHAR(100), @ErrorID NVARCHAR(36), @PartitionNum INT; 
	DECLARE @CurrentUserSpot INT, @WhiteSpaceSpot INT, @ErrorIDSpot INT, @PartitionNumSpot INT;
	DECLARE @SummaryMessage NVARCHAR(1500);
	DECLARE @ClassNm VARCHAR(200), @AssemblyName VARCHAR(500), @MethodNm VARCHAR(200), @ExecutableNm VARCHAR(300);

	SET @UserName = NULL;
	SELECT @CurrentUserSpot = CHARINDEX('CurrentUser:', @FormattedMessage);
	IF @CurrentUserSpot > 0
	BEGIN
		SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @CurrentUserSpot +  13);
		IF @WhiteSpaceSpot > 0
		BEGIN
			SELECT @UserName = SUBSTRING(@FormattedMessage, @CurrentUserSpot + 13, @WhiteSpaceSpot - (@CurrentUserSpot + 13));
		END;
	END;
       
	SET @ErrorID = NULL;
	SELECT @ErrorIDSpot = CHARINDEX('ErrorId:', @FormattedMessage);
	IF @ErrorIDSpot > 0
	BEGIN
		SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @ErrorIDSpot +  9);
		-- SELECT @ErrorIDSpot EIS, @WhiteSpaceSpot WSS
		IF @WhiteSpaceSpot > 0 
		BEGIN
			SELECT @ErrorID = SUBSTRING(@FormattedMessage, @ErrorIDSpot + 9, @WhiteSpaceSpot - (@ErrorIDSpot + 9));
		END;
		ELSE 
		BEGIN
			SELECT @ErrorID = SUBSTRING(@FormattedMessage, @ErrorIDSpot + 9, LEN(@FormattedMessage) - (@ErrorIDSpot + 9));
		END;
	END;    

    SET @SummaryMessage = NULL;
    SELECT @PartitionNumSpot = CHARINDEX('SummaryMessage:', @FormattedMessage);
    IF @PartitionNumSpot > 0
    BEGIN
		SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @PartitionNumSpot +  16);
        SELECT @PartitionNumSpot EIS, @WhiteSpaceSpot WSS
		IF @WhiteSpaceSpot > 0 
		BEGIN
			SELECT @SummaryMessage = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 16, @WhiteSpaceSpot - (@PartitionNumSpot + 16));
        END;
        ELSE 
        BEGIN
            SELECT @SummaryMessage = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 16, LEN(@FormattedMessage) - (@PartitionNumSpot + 16));
        END;
    END;

    SET @AssemblyName = NULL;
    SELECT @PartitionNumSpot = CHARINDEX('Assembly:', @FormattedMessage);
    IF @PartitionNumSpot > 0
    BEGIN
        SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @PartitionNumSpot +  10);
        SELECT @PartitionNumSpot EIS, @WhiteSpaceSpot WSS
        IF @WhiteSpaceSpot > 0 
        BEGIN
            SELECT @AssemblyName = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 10, @WhiteSpaceSpot - (@PartitionNumSpot + 10));
        END;
        ELSE 
        BEGIN
            SELECT @AssemblyName = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 10, LEN(@FormattedMessage) - (@PartitionNumSpot + 10));
        END;
	END;

    SET @ClassNm = NULL;
    SELECT @PartitionNumSpot = CHARINDEX('Class:', @FormattedMessage);
	IF @PartitionNumSpot > 0
	BEGIN
        SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @PartitionNumSpot +  7);
	    SELECT @PartitionNumSpot EIS, @WhiteSpaceSpot WSS
        IF @WhiteSpaceSpot > 0 
        BEGIN
            SELECT @ClassNm = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 7, @WhiteSpaceSpot - (@PartitionNumSpot + 7));
        END;
        ELSE 
        BEGIN
            SELECT @ClassNm = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 7, LEN(@FormattedMessage) - (@PartitionNumSpot + 7));
        END;
	END;
	
    SET @MethodNm = NULL;
    SELECT @PartitionNumSpot = CHARINDEX('Method:', @FormattedMessage);
    IF @PartitionNumSpot > 0
    BEGIN
        SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @PartitionNumSpot +  8);
        SELECT @PartitionNumSpot EIS, @WhiteSpaceSpot WSS
        IF @WhiteSpaceSpot > 0 
        BEGIN
            SELECT @MethodNm = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 8, @WhiteSpaceSpot - (@PartitionNumSpot + 8));
        END;
        ELSE 
        BEGIN
            SELECT @MethodNm = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 8, LEN(@FormattedMessage) - (@PartitionNumSpot + 8));
        END;
    END;

    SET @ExecutableNm = NULL;
    SELECT @PartitionNumSpot = CHARINDEX('Executable:', @FormattedMessage);
    IF @PartitionNumSpot > 0
    BEGIN
        SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @PartitionNumSpot +  12);
        SELECT @PartitionNumSpot EIS, @WhiteSpaceSpot WSS
        IF @WhiteSpaceSpot > 0 
        BEGIN
            SELECT @ExecutableNm = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 12, @WhiteSpaceSpot - (@PartitionNumSpot + 12));
        END;
        ELSE 
        BEGIN
            SELECT @ExecutableNm = SUBSTRING(@FormattedMessage, @PartitionNumSpot + 12, LEN(@FormattedMessage) - (@PartitionNumSpot + 12));
        END;
    END;

	-- Clean up formatted message by removing stuff we're showing elsewhere
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'SummaryMessage: ' + ISNULL(@SummaryMessage, '') +  CHAR(13) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'CurrentUser: ' + ISNULL(@UserName, '') +  CHAR(13) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Priority: 0' + CHAR(13) + CHAR(10) +  CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'EventId: 0' + CHAR(13) + CHAR(10) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Severity: Information' + CHAR(13) + CHAR(10) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'App Domain: msmqDistributor.exe' + CHAR(13) + CHAR(10) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Title:' + ISNULL(@Title, '') + CHAR(13) + CHAR(10) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Machine: ' + ISNULL(@MachineName, '') + CHAR(13) + CHAR(10) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Assembly: ' + ISNULL(@AssemblyName, '') + CHAR(13) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Class: ' + ISNULL(@ClassNm, '') + CHAR(13) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Method: ' + ISNULL(@MethodNm, '') + CHAR(13) + CHAR(10), '')
	SET @FormattedMessage = REPLACE(@FormattedMessage, 'Executable: ' + ISNULL(@ExecutableNm, '') + CHAR(13) + CHAR(10), '')

	DECLARE @CategoryStart INT, @CategoryStop INT;
	SELECT @CategoryStart = CHARINDEX('Category:', @FormattedMessage) -- location of first character of "Category:"
	SELECT @CategoryStop = CHARINDEX (CHAR(13), @FormattedMessage, @CategoryStart)  -- location of end-of-line following Category":"
	-- add 1 for being inclusive from the start to the stop character locations
	-- add 1 for the CR
	-- add 2 for the double LF
	-- "+ 4" total
	SELECT @FormattedMessage = STUFF(@FormattedMessage, @CategoryStart, @CategoryStop - @CategoryStart + 4, '')

	SET @FormattedMessage = @FormattedMessage + CHAR(13) + CHAR(10)

	INSERT INTO [EntLib].[xtLog] (
		[EventID],
		[Priority],
		[Severity],
		[Title],
		[Timestamp],
		[MachineName],
		[AssemblyName],
		[MethodName],
		[ClassName],
		[ThreadName],
		[ExecutableName],
		[Message],
		[FormattedMessage],
		[UserName],
		[ErrorId]		
	) VALUES (
		@EventID, 
		@Priority, 
		@Severity, 
		@Title, 
		GETDATE(),
		@MachineName, 
		ISNULL(@AssemblyName, ''),
		ISNULL(@MethodNm, ''),
		ISNULL(@ClassNm, ''),
		@ThreadName,
		@ExecutableNm,
		@SummaryMessage,
		@FormattedMessage,
		@UserName,
		@ErrorID		
	);

    SET @LogID = SCOPE_IDENTITY();
    RETURN @LogID;