USE [master]
GO

IF  EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'SitecoreCommerce_SharedEnvironments')
DROP DATABASE [SitecoreCommerce_SharedEnvironments]
GO

IF  EXISTS (SELECT name FROM master.dbo.sysdatabases WHERE name = N'SitecoreCommerce_Global')
DROP DATABASE [SitecoreCommerce_Global]
GO

CREATE DATABASE [SitecoreCommerce_SharedEnvironments]
GO

ALTER DATABASE [SitecoreCommerce_SharedEnvironments] MODIFY FILE
( NAME = N'SitecoreCommerce_SharedEnvironments' , SIZE = 1GB , MAXSIZE = 6GB, FILEGROWTH = 1GB )
GO

ALTER DATABASE [SitecoreCommerce_SharedEnvironments] MODIFY FILE
( NAME = N'SitecoreCommerce_SharedEnvironments_log' , SIZE = 100MB , MAXSIZE = 1GB , FILEGROWTH = 20MB)
GO

ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET COMPATIBILITY_LEVEL = 110
GO

ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET ARITHABORT OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET AUTO_UPDATE_STATISTICS ON 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET DISABLE_BROKER 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET MULTI_USER 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
/******ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET DELAYED_DURABILITY = DISABLED 
GO ******/
USE [SitecoreCommerce_SharedEnvironments]
GO

/****** Object:  Table [dbo].[Versions]    Script Date: 4/26/2016 11:46:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Versions](
	[DBVersion] [nvarchar](50) NULL
) ON [PRIMARY]

GO

INSERT INTO [dbo].[Versions] ([DBVersion]) VALUES ('0.1.0')
GO

/****** Object:  Table [dbo].[CommerceEntities]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommerceEntities](
	[Id] [nvarchar](150) NOT NULL,
	[EnvironmentId] [uniqueidentifier] NOT NULL,
	[Version] [int],
	[Entity] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CommerceEntities] PRIMARY KEY CLUSTERED 
(
	[EnvironmentId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO

/****** Object:  Table [dbo].[CommerceLists]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommerceLists](
	[ListName] [nvarchar](150) NOT NULL,
	[EnvironmentId] [uniqueidentifier] NOT NULL,
	[CommerceEntityId] [nvarchar](150) NOT NULL
) ON [PRIMARY]

GO


SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CommerceLists]    Script Date: 12/17/2015 6:01:37 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CommerceLists] ON [dbo].[CommerceLists]
(
	[EnvironmentId] ASC,
	[ListName] ASC,
	[CommerceEntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[sp_CleanEnvironment]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CleanEnvironment]
(
	@EnvironmentId uniqueidentifier
)
as

set nocount on
delete from [CommerceLists] Where EnvironmentId = @EnvironmentId
delete from [CommerceEntities] Where EnvironmentId = @EnvironmentId


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesDelete]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[sp_CommerceEntitiesDelete]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[CommerceEntityId] = @Id 
	

delete from 
	[CommerceEntities]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id 
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesInsert]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceEntitiesInsert]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Version int,
	@Entity nvarchar(max)
)

as

set nocount on

insert into [CommerceEntities]
(
	[Id],
	[EnvironmentId],
	[Version],
	[Entity]
)
values
(
	@Id,
	@EnvironmentId,
	@Version,
	@Entity
)


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelect]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[sp_CommerceEntitiesSelect]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	[Entity]
from 
	[CommerceEntities]
with (nolock)
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id 
	

GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesUpdate]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceEntitiesUpdate]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Version int,
	@Entity nvarchar(max)
)

as

set nocount on

/* Get the existing version of the entity */
declare @currentVersion int
SET @currentVersion = (select [Version] from [CommerceEntities] with (nolock) where [EnvironmentId] = @EnvironmentId AND [Id] = @Id);

/* If the version supplied is lower or equal to the current version, then we raise an error*/
IF (@Version <= @currentVersion)
BEGIN
	DECLARE @ErrorMsg NVARCHAR(2048) = FORMATMESSAGE('Concurency error: The Entity version supplied (%i) is lower or equal to the current version (%i).', @Version, @currentVersion);
	THROW 50000, @ErrorMsg, 1;
END
ELSE
BEGIN
update [CommerceEntities]
set 
	[Entity] = @Entity,
	[Version] = @Version
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id 
END

GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsCount]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsCount]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	COUNT(CommerceEntityId)
from 
	[CommerceLists]
with
	(NOLOCK)
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName 
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDelete]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsDelete]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName 
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDeleteEntity]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsDeleteEntity]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Id nvarchar(150)
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
	AND
	[CommerceEntityId] = @Id
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsInsert]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsInsert]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@CommerceEntityId nvarchar(150)
)

as

set nocount on

-- If the list entry already exists, do not duplicate it
IF NOT EXISTS (SELECT [ListName],[EnvironmentId],[CommerceEntityId] FROM [SitecoreCommerce_SharedEnvironments].[dbo].[CommerceLists] 
WITH (updlock, rowlock, holdlock)
WHERE [EnvironmentId] = @EnvironmentId AND [ListName] = @ListName AND [CommerceEntityId] = @CommerceEntityId )
BEGIN
	insert into [CommerceLists]
	(
		[ListName],
		[EnvironmentId],
		[CommerceEntityId]
	)
	values
	(
		@ListName,
		@EnvironmentId,
		@CommerceEntityId
	)
END



GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelect]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsSelect]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	[ListName],
	[EnvironmentId],
	[CommerceEntityId]
from 
	[CommerceLists]
WITH (NOLOCK)
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName 
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectAll]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO


/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectByRange]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsSelectByRange]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Skip int = 0,
	@Take int = 2,
	@SortOrder int = 0
)

as

set nocount on

SELECT 
	[CommerceEntityId] FROM CommerceLists
WITH (NOLOCK)
WHERE 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName 
	
ORDER BY 
CASE WHEN @SortOrder = 0  THEN
	[CommerceEntityId] END ASC, 
CASE WHEN @SortOrder = 1 THEN 
	[CommerceEntityId] END DESC
	OFFSET @Skip ROWS
	FETCH NEXT @Take ROWS ONLY

GO

USE [master]
GO
ALTER DATABASE [SitecoreCommerce_SharedEnvironments] SET  READ_WRITE 
GO

USE [master]
	
	DECLARE @databaseName VARCHAR(100) 
	DECLARE @userName VARCHAR(100)
	DECLARE @roleName VARCHAR(100)
	DECLARE @dynamicSQL VARCHAR(MAX)

	SET @databaseName = 'SitecoreCommerce_SharedEnvironments'
	SET @userName = HOST_NAME() + '\HabitatRuntimeUser'
	SET @roleName = 'db_owner'

	/* If the user does not exist as a login, add it to the system security */	
	IF NOT EXISTS(SELECT name FROM master.dbo.syslogins WHERE name = @userName)
	BEGIN
		PRINT 'Creating login ' + @userName
		EXEC( 'CREATE LOGIN [' + @userName + '] FROM WINDOWS' )
	END
	
	
	SET @dynamicSQL = 'USE [' + @databaseName + ']; ' +
		'DECLARE @isLogin bit;' +
		'IF  NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @userName + ''') ' +
		'BEGIN ' +
			'PRINT ''Creating user ' + @userName + '''; ' +
			' CREATE USER [' + @userName + '] FOR LOGIN [' + @userName + '];' +
		'END ' +
		'EXECUTE AS LOGIN = ''' + @userName + ''';' +
			'SET @isLogin = IS_MEMBER(''' + @roleName + ''');' +
		'REVERT;' +
		
		'IF @isLogin = 0 ' +
		'BEGIN ' +
			'PRINT ''Adding user ' + @userName + ' to role ' + @roleName + ''';' +
			'EXEC sp_addrolemember ''' + @roleName + ''', ''' + @userName + ''';' +
		'END;'
	EXEC( @dynamicSQL)
	
CREATE DATABASE [SitecoreCommerce_Global]
GO

ALTER DATABASE [SitecoreCommerce_Global] MODIFY FILE
( NAME = N'SitecoreCommerce_Global' , SIZE = 100MB , MAXSIZE = 1GB, FILEGROWTH = 5024KB )
GO

ALTER DATABASE [SitecoreCommerce_Global] MODIFY FILE
( NAME = N'SitecoreCommerce_Global_log' , SIZE = 50MB , MAXSIZE = 1GB , FILEGROWTH = 5024KB )
GO

ALTER DATABASE [SitecoreCommerce_Global] SET COMPATIBILITY_LEVEL = 110
GO

ALTER DATABASE [SitecoreCommerce_Global] SET ANSI_NULL_DEFAULT OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET ANSI_NULLS OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET ANSI_PADDING OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET ANSI_WARNINGS OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET ARITHABORT OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET AUTO_CLOSE OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET AUTO_SHRINK OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET AUTO_UPDATE_STATISTICS OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET CURSOR_CLOSE_ON_COMMIT OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET CURSOR_DEFAULT  GLOBAL 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET CONCAT_NULL_YIELDS_NULL OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET NUMERIC_ROUNDABORT OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET QUOTED_IDENTIFIER OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET RECURSIVE_TRIGGERS OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET  DISABLE_BROKER 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET AUTO_UPDATE_STATISTICS_ASYNC OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET DATE_CORRELATION_OPTIMIZATION OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET TRUSTWORTHY OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET ALLOW_SNAPSHOT_ISOLATION OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET PARAMETERIZATION SIMPLE 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET READ_COMMITTED_SNAPSHOT OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET HONOR_BROKER_PRIORITY OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET RECOVERY SIMPLE 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET  MULTI_USER 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET PAGE_VERIFY CHECKSUM  
GO
ALTER DATABASE [SitecoreCommerce_Global] SET DB_CHAINING OFF 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET FILESTREAM( NON_TRANSACTED_ACCESS = OFF ) 
GO
ALTER DATABASE [SitecoreCommerce_Global] SET TARGET_RECOVERY_TIME = 0 SECONDS 
GO
/******ALTER DATABASE [SitecoreCommerce_Global] SET DELAYED_DURABILITY = DISABLED 
GO ******/
USE [SitecoreCommerce_Global]
GO

/****** Object:  Table [dbo].[Versions]    Script Date: 4/26/2016 11:46:17 AM ******/
SET ANSI_NULLS ON
GO

SET QUOTED_IDENTIFIER ON
GO

CREATE TABLE [dbo].[Versions](
	[DBVersion] [nvarchar](50) NULL
) ON [PRIMARY]

INSERT INTO [dbo].[Versions] ([DBVersion]) VALUES ('0.1.0')
GO


GO
/****** Object:  Table [dbo].[CommerceEntities]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommerceEntities](
	[Id] [nvarchar](150) NOT NULL,
	[EnvironmentId] [uniqueidentifier] NOT NULL,
	[Version] [int],
	[Entity] [nvarchar](max) NOT NULL,
 CONSTRAINT [PK_CommerceEntities] PRIMARY KEY CLUSTERED 
(
	[EnvironmentId] ASC,
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]

GO
/****** Object:  Table [dbo].[CommerceLists]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CommerceLists](
	[ListName] [nvarchar](150) NOT NULL,
	[EnvironmentId] [uniqueidentifier] NOT NULL,
	[CommerceEntityId] [nvarchar](150) NOT NULL
) ON [PRIMARY]

GO
SET ANSI_PADDING ON

GO
/****** Object:  Index [IX_CommerceLists]    Script Date: 12/17/2015 6:01:37 PM ******/
CREATE UNIQUE NONCLUSTERED INDEX [IX_CommerceLists] ON [dbo].[CommerceLists]
(
	[EnvironmentId] ASC,
	[ListName] ASC,
	[CommerceEntityId] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, SORT_IN_TEMPDB = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = OFF) ON [PRIMARY]
GO
/****** Object:  StoredProcedure [dbo].[sp_CleanEnvironment]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CleanEnvironment]
(
	@EnvironmentId uniqueidentifier
)
as

set nocount on
delete from [CommerceLists] Where EnvironmentId = @EnvironmentId
delete from [CommerceEntities] Where EnvironmentId = @EnvironmentId

GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesDelete]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[sp_CommerceEntitiesDelete]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[CommerceEntityId] = @Id

delete from 
	[CommerceEntities]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesInsert]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceEntitiesInsert]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Version int,
	@Entity nvarchar(max)
)

as

set nocount on

insert into [CommerceEntities]
(
	[Id],
	[EnvironmentId],
	[Version],
	[Entity]
)
values
(
	@Id,
	@EnvironmentId,
	@Version,
	@Entity
)


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesSelect]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

CREATE procedure [dbo].[sp_CommerceEntitiesSelect]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	[Entity]
from 
	[CommerceEntities]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id
	


GO

/****** Object:  StoredProcedure [dbo].[sp_CommerceEntitiesUpdate]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceEntitiesUpdate]
(
	@Id nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Version int,
	@Entity nvarchar(max)
)

as

set nocount on

update 
	[CommerceEntities]
set 
	[Entity] = @Entity,
	[Version] = @Version
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[Id] = @Id 


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsCount]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsCount]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	COUNT(CommerceEntityId)
from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDelete]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsDelete]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsDeleteEntity]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsDeleteEntity]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Id nvarchar(150)
)

as

set nocount on

delete from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
	AND
	[CommerceEntityId] = @Id


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsInsert]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsInsert]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@CommerceEntityId nvarchar(150)
)

as

set nocount on

-- If the list entry already exists, do not duplicate it
IF NOT EXISTS (SELECT [ListName], [EnvironmentId],[CommerceEntityId] FROM [SitecoreCommerce_Global].[dbo].[CommerceLists] 
WITH (updlock, rowlock, holdlock)
WHERE [ListName] = @ListName  and [CommerceEntityId] = @CommerceEntityId AND [EnvironmentId] = @EnvironmentId)
BEGIN
	insert into [CommerceLists]
	(
		[ListName],
		[EnvironmentId],
		[CommerceEntityId]
	)
	values
	(
		@ListName,
		@EnvironmentId,
		@CommerceEntityId
	)
END



GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelect]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsSelect]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier
)

as

set nocount on

select 
	[ListName],
	[EnvironmentId],
	[CommerceEntityId]
from 
	[CommerceLists]
where 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
	


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectAll]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsSelectAll]

as

set nocount on

select 
	[ListName],
	[EnvironmentId],
	[CommerceEntityId]
from 
	[CommerceLists]


GO
/****** Object:  StoredProcedure [dbo].[sp_CommerceListsSelectByRange]    Script Date: 12/17/2015 6:01:37 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO

create procedure [dbo].[sp_CommerceListsSelectByRange]
(
	@ListName nvarchar(150),
	@EnvironmentId uniqueidentifier,
	@Skip int = 0,
	@Take int = 2,
	@SortOrder int = 0
)

as

set nocount on

SELECT 
	[CommerceEntityId] FROM CommerceLists
WHERE 
	[EnvironmentId] = @EnvironmentId
	AND
	[ListName] = @ListName
	
ORDER BY 
CASE WHEN @SortOrder = 0  THEN
	[CommerceEntityId] END ASC, 
CASE WHEN @SortOrder = 1 THEN 
	[CommerceEntityId] END DESC
	OFFSET @Skip ROWS
FETCH NEXT @Take ROWS ONLY

GO


USE [master]
GO
ALTER DATABASE [SitecoreCommerce_Global] SET  READ_WRITE 
GO

USE [master]
	
	DECLARE @databaseName VARCHAR(100) 
	DECLARE @userName VARCHAR(100)
	DECLARE @roleName VARCHAR(100)
	DECLARE @dynamicSQL VARCHAR(MAX)

	SET @databaseName = 'SitecoreCommerce_Global'
	SET @userName = HOST_NAME() + '\HabitatRuntimeUser'
	SET @roleName = 'db_owner'

	/* If the user does not exist as a login, add it to the system security */	
	IF NOT EXISTS(SELECT name FROM master.dbo.syslogins WHERE name = @userName)
	BEGIN
		PRINT 'Creating login ' + @userName
		EXEC( 'CREATE LOGIN [' + @userName + '] FROM WINDOWS' )
	END
	
	
	SET @dynamicSQL = 'USE [' + @databaseName + ']; ' +
		'DECLARE @isLogin bit;' +
		'IF  NOT EXISTS (SELECT * FROM sys.database_principals WHERE name = ''' + @userName + ''') ' +
		'BEGIN ' +
			'PRINT ''Creating user ' + @userName + '''; ' +
			' CREATE USER [' + @userName + '] FOR LOGIN [' + @userName + '];' +
		'END ' +
		'EXECUTE AS LOGIN = ''' + @userName + ''';' +
			'SET @isLogin = IS_MEMBER(''' + @roleName + ''');' +
		'REVERT;' +
		
		'IF @isLogin = 0 ' +
		'BEGIN ' +
			'PRINT ''Adding user ' + @userName + ' to role ' + @roleName + ''';' +
			'EXEC sp_addrolemember ''' + @roleName + ''', ''' + @userName + ''';' +
		'END;'
	EXEC( @dynamicSQL)	