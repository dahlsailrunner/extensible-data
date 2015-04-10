CREATE PROCEDURE EntLib.xspWritePerfLog
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
    DECLARE @UserName NVARCHAR(100) = NULL
		   ,@PerfName NVARCHAR(100) = NULL  
		   ,@BeginDs DATETIME = NULL
		   ,@ElapsedMilliseconds INT = NULL;

    DECLARE  @CurrentSpot INT
			,@CurrWorkingLength INT  
			,@WhiteSpaceSpot INT       
			,@TextToLocate VARCHAR(100);

    DECLARE @NewLine CHAR(2);

	SET @NewLine = CHAR(13) + CHAR(10);
	SET @TextToLocate = 'PerfName:'
	SET @CurrWorkingLength = LEN(@TextToLocate) + 1    
    SET @CurrentSpot = CHARINDEX(@TextToLocate, @FormattedMessage);
    IF @CurrentSpot > 0 
	BEGIN
		SELECT @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @CurrentSpot + @CurrWorkingLength);
		IF @WhiteSpaceSpot > 0 
			SET @PerfName = SUBSTRING(@FormattedMessage, @CurrentSpot + @CurrWorkingLength, @WhiteSpaceSpot - ( @CurrentSpot + @CurrWorkingLength ));
	END;

	SET @TextToLocate = 'CurrentUser:'
	SET @CurrWorkingLength = LEN(@TextToLocate) + 1;
    SET @CurrentSpot = CHARINDEX(@TextToLocate, @FormattedMessage);
    IF @CurrentSpot > 0 
	BEGIN
		SET @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @CurrentSpot + @CurrWorkingLength);
        IF @WhiteSpaceSpot > 0 
			SET @UserName = SUBSTRING(@FormattedMessage, @CurrentSpot + @CurrWorkingLength, @WhiteSpaceSpot - ( @CurrentSpot + @CurrWorkingLength ));
	END;

	SET @TextToLocate = 'Started:'
	SET @CurrWorkingLength = LEN(@TextToLocate) + 1    
    SELECT  @CurrentSpot = CHARINDEX(@TextToLocate, @FormattedMessage);
    IF @CurrentSpot > 0 
	BEGIN
		SET @WhiteSpaceSpot = CHARINDEX(@NewLine, @FormattedMessage, @CurrentSpot + @CurrWorkingLength);
		IF @WhiteSpaceSpot > 0 
			SET @BeginDs = CONVERT(DATETIME, SUBSTRING(@FormattedMessage, @CurrentSpot + @CurrWorkingLength, @WhiteSpaceSpot - ( @CurrentSpot + @CurrWorkingLength )));
	END;

	SET @TextToLocate = 'ElapsedMilliseconds:'
	SET @CurrWorkingLength = LEN(@TextToLocate) + 1    
    SELECT  @CurrentSpot = CHARINDEX(@TextToLocate, @FormattedMessage);
    IF @CurrentSpot > 0 
	BEGIN
		SET @WhiteSpaceSpot = CHARINDEX(CHAR(13), @FormattedMessage, @CurrentSpot + @CurrWorkingLength);
		IF @WhiteSpaceSpot > 0 
			SET @ElapsedMilliseconds = SUBSTRING(@FormattedMessage, @CurrentSpot + @CurrWorkingLength, @WhiteSpaceSpot - ( @CurrentSpot + @CurrWorkingLength ));
	END;

    INSERT INTO [EntLib].[xtPerfLog] ( 
		[PerfNm]
		,[BeginDs]
		,[MachineNm]
		,[UserName]
		,[ElapsedMilliseconds]
		,[AddtlInfo]
	) VALUES (
		@PerfName
		,@BeginDs
		,@MachineName
		,@UserName 
		,@ElapsedMilliseconds      
		,@FormattedMessage     
	);

    SET @LogID = SCOPE_IDENTITY();
    RETURN @LogID;