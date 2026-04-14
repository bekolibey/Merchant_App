using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace merchantapp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignExistingMerchantDb : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH('MerchantApplications', 'CompanyName') IS NOT NULL
                    EXEC sp_rename 'MerchantApplications.CompanyName', 'WorkplaceSignboardName', 'COLUMN';
                IF COL_LENGTH('MerchantApplications', 'AuthorizedPersonName') IS NOT NULL
                    EXEC sp_rename 'MerchantApplications.AuthorizedPersonName', 'ManagerFullName', 'COLUMN';
                IF COL_LENGTH('MerchantApplications', 'AuthorizedPersonIdNumber') IS NOT NULL
                    EXEC sp_rename 'MerchantApplications.AuthorizedPersonIdNumber', 'IdentityNumber', 'COLUMN';
                IF COL_LENGTH('MerchantApplications', 'MobilePhone') IS NOT NULL
                    EXEC sp_rename 'MerchantApplications.MobilePhone', 'GsmNumber', 'COLUMN';
                IF COL_LENGTH('MerchantApplications', 'AddressDetail') IS NOT NULL
                    EXEC sp_rename 'MerchantApplications.AddressDetail', 'CustomerAddress', 'COLUMN';
                IF COL_LENGTH('MerchantApplications', 'BusinessCategory') IS NOT NULL
                    EXEC sp_rename 'MerchantApplications.BusinessCategory', 'CompanyType', 'COLUMN';
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('MerchantApplications', 'ApplicationNumber') IS NULL ALTER TABLE MerchantApplications ADD ApplicationNumber varchar(30) NOT NULL CONSTRAINT DF_MerchantApplications_ApplicationNumber DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'WorkflowReferenceNumber') IS NULL ALTER TABLE MerchantApplications ADD WorkflowReferenceNumber varchar(50) NOT NULL CONSTRAINT DF_MerchantApplications_WorkflowReferenceNumber DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'BranchCode') IS NULL ALTER TABLE MerchantApplications ADD BranchCode varchar(20) NOT NULL CONSTRAINT DF_MerchantApplications_BranchCode DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'BranchName') IS NULL ALTER TABLE MerchantApplications ADD BranchName varchar(150) NOT NULL CONSTRAINT DF_MerchantApplications_BranchName DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'CustomerNumber') IS NULL ALTER TABLE MerchantApplications ADD CustomerNumber varchar(30) NOT NULL CONSTRAINT DF_MerchantApplications_CustomerNumber DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'IsSpecialRulingRequired') IS NULL ALTER TABLE MerchantApplications ADD IsSpecialRulingRequired bit NOT NULL CONSTRAINT DF_MerchantApplications_IsSpecialRulingRequired DEFAULT 0;
                IF COL_LENGTH('MerchantApplications', 'DemandDepositAccountNumber') IS NULL ALTER TABLE MerchantApplications ADD DemandDepositAccountNumber varchar(34) NOT NULL CONSTRAINT DF_MerchantApplications_DemandDepositAccountNumber DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'ContractedUserCode') IS NULL ALTER TABLE MerchantApplications ADD ContractedUserCode varchar(30) NOT NULL CONSTRAINT DF_MerchantApplications_ContractedUserCode DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'TradeRegistryNumber') IS NULL ALTER TABLE MerchantApplications ADD TradeRegistryNumber varchar(30) NOT NULL CONSTRAINT DF_MerchantApplications_TradeRegistryNumber DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'TradeRegistryRegistrationDate') IS NULL ALTER TABLE MerchantApplications ADD TradeRegistryRegistrationDate datetime2 NULL;
                IF COL_LENGTH('MerchantApplications', 'TradeRegistryRegistrationName') IS NULL ALTER TABLE MerchantApplications ADD TradeRegistryRegistrationName varchar(150) NOT NULL CONSTRAINT DF_MerchantApplications_TradeRegistryRegistrationName DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'PosInstallationAddress') IS NULL ALTER TABLE MerchantApplications ADD PosInstallationAddress varchar(500) NOT NULL CONSTRAINT DF_MerchantApplications_PosInstallationAddress DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'PostalCode') IS NULL ALTER TABLE MerchantApplications ADD PostalCode varchar(10) NOT NULL CONSTRAINT DF_MerchantApplications_PostalCode DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'OwnerIdentityNumber') IS NULL ALTER TABLE MerchantApplications ADD OwnerIdentityNumber varchar(11) NOT NULL CONSTRAINT DF_MerchantApplications_OwnerIdentityNumber DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'OwnerFullName') IS NULL ALTER TABLE MerchantApplications ADD OwnerFullName varchar(150) NOT NULL CONSTRAINT DF_MerchantApplications_OwnerFullName DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'OwnerMobilePhone') IS NULL ALTER TABLE MerchantApplications ADD OwnerMobilePhone varchar(15) NOT NULL CONSTRAINT DF_MerchantApplications_OwnerMobilePhone DEFAULT '';
                IF COL_LENGTH('MerchantApplications', 'UpdatedAt') IS NULL ALTER TABLE MerchantApplications ADD UpdatedAt datetime2 NULL;
                """);

            migrationBuilder.Sql(
                """
                UPDATE MerchantApplications
                SET
                    ApplicationNumber = CASE WHEN NULLIF(ApplicationNumber, '') IS NULL THEN LEFT('APP-' + REPLACE(CONVERT(varchar(36), Id), '-', ''), 30) ELSE ApplicationNumber END,
                    WorkflowReferenceNumber = CASE WHEN NULLIF(WorkflowReferenceNumber, '') IS NULL THEN LEFT('WF-' + REPLACE(CONVERT(varchar(36), Id), '-', ''), 50) ELSE WorkflowReferenceNumber END,
                    BranchCode = CASE WHEN NULLIF(BranchCode, '') IS NULL THEN '0001' ELSE BranchCode END,
                    BranchName = CASE WHEN NULLIF(BranchName, '') IS NULL THEN 'Merkez Sube' ELSE BranchName END,
                    CustomerNumber = CASE WHEN NULLIF(CustomerNumber, '') IS NULL THEN LEFT('CUST-' + REPLACE(CONVERT(varchar(36), Id), '-', ''), 30) ELSE CustomerNumber END,
                    DemandDepositAccountNumber = CASE WHEN NULLIF(DemandDepositAccountNumber, '') IS NULL THEN LEFT('TR' + REPLACE(CONVERT(varchar(36), Id), '-', ''), 34) ELSE DemandDepositAccountNumber END,
                    ContractedUserCode = CASE WHEN NULLIF(ContractedUserCode, '') IS NULL THEN 'LEGACY' ELSE ContractedUserCode END,
                    TradeRegistryNumber = CASE WHEN NULLIF(TradeRegistryNumber, '') IS NULL THEN ISNULL(TaxNumber, '') ELSE TradeRegistryNumber END,
                    TradeRegistryRegistrationName = CASE WHEN NULLIF(TradeRegistryRegistrationName, '') IS NULL THEN ISNULL(WorkplaceSignboardName, '') ELSE TradeRegistryRegistrationName END,
                    PosInstallationAddress = CASE WHEN NULLIF(PosInstallationAddress, '') IS NULL THEN ISNULL(CustomerAddress, '') ELSE PosInstallationAddress END,
                    PostalCode = CASE WHEN NULLIF(PostalCode, '') IS NULL THEN '34000' ELSE PostalCode END,
                    OwnerIdentityNumber = CASE WHEN NULLIF(OwnerIdentityNumber, '') IS NULL THEN ISNULL(IdentityNumber, '') ELSE OwnerIdentityNumber END,
                    OwnerFullName = CASE WHEN NULLIF(OwnerFullName, '') IS NULL THEN ISNULL(ManagerFullName, '') ELSE OwnerFullName END,
                    OwnerMobilePhone = CASE WHEN NULLIF(OwnerMobilePhone, '') IS NULL THEN ISNULL(GsmNumber, '') ELSE OwnerMobilePhone END,
                    UpdatedAt = COALESCE(UpdatedAt, CreatedAt),
                    ApplicationStatus = CASE WHEN NULLIF(ApplicationStatus, '') IS NULL THEN N'Onay Bekliyor' ELSE ApplicationStatus END;
                """);

            migrationBuilder.Sql(
                """
                ALTER TABLE MerchantApplications ALTER COLUMN Latitude decimal(9,6) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN Longitude decimal(9,6) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN ApplicationNumber varchar(30) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN WorkflowReferenceNumber varchar(50) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN BranchCode varchar(20) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN BranchName varchar(150) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN CustomerNumber varchar(30) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN IdentityNumber varchar(11) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN CompanyType varchar(50) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN TaxNumber varchar(10) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN DemandDepositAccountNumber varchar(34) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN WorkplaceSignboardName varchar(150) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN ContractedUserCode varchar(30) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN TradeRegistryNumber varchar(30) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN ManagerFullName varchar(150) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN TradeRegistryRegistrationName varchar(150) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN TaxOffice varchar(50) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN CustomerAddress varchar(500) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN PosInstallationAddress varchar(500) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN Email varchar(150) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN City varchar(50) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN District varchar(50) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN PostalCode varchar(10) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN GsmNumber varchar(15) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN WebAddress varchar(100) NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN OwnerIdentityNumber varchar(11) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN OwnerFullName varchar(150) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN OwnerMobilePhone varchar(15) NOT NULL;
                ALTER TABLE MerchantApplications ALTER COLUMN ApplicationStatus varchar(20) NOT NULL;
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'MerchantApplicationHistories', N'U') IS NULL
                BEGIN
                    CREATE TABLE MerchantApplicationHistories (
                        Id uniqueidentifier NOT NULL,
                        MerchantApplicationId uniqueidentifier NOT NULL,
                        ProcessDate datetime2 NOT NULL,
                        Description varchar(200) NOT NULL,
                        Status varchar(30) NOT NULL,
                        UserCode varchar(30) NOT NULL,
                        HistoryDescription varchar(500) NOT NULL,
                        CONSTRAINT PK_MerchantApplicationHistories PRIMARY KEY (Id),
                        CONSTRAINT FK_MerchantApplicationHistories_MerchantApplications_MerchantApplicationId FOREIGN KEY (MerchantApplicationId) REFERENCES MerchantApplications(Id) ON DELETE CASCADE
                    );
                    CREATE INDEX IX_MerchantApplicationHistories_MerchantApplicationId ON MerchantApplicationHistories(MerchantApplicationId);
                END
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('MerchantApplications', 'StatusNote') IS NOT NULL
                AND NOT EXISTS (SELECT 1 FROM MerchantApplicationHistories)
                BEGIN
                    INSERT INTO MerchantApplicationHistories (Id, MerchantApplicationId, ProcessDate, Description, Status, UserCode, HistoryDescription)
                    SELECT NEWID(),
                           Id,
                           COALESCE(LastStatusChangedAt, UpdatedAt, CreatedAt),
                           CASE WHEN NULLIF(StatusNote, '') IS NULL THEN N'Legacy kayit' ELSE LEFT(StatusNote, 200) END,
                           LEFT(ApplicationStatus, 30),
                           'LEGACY',
                           N'Eski tablodan aktarildi'
                    FROM MerchantApplications;
                END
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'ExchangeRateSnapshots', N'U') IS NULL
                BEGIN
                    CREATE TABLE ExchangeRateSnapshots (
                        Id uniqueidentifier NOT NULL,
                        UsdToTryRate decimal(18,4) NULL,
                        EurToTryRate decimal(18,4) NULL,
                        FetchedAtUtc datetime2 NULL,
                        Source varchar(100) NOT NULL,
                        CONSTRAINT PK_ExchangeRateSnapshots PRIMARY KEY (Id)
                    );
                END
                IF NOT EXISTS (SELECT 1 FROM ExchangeRateSnapshots WHERE Id = 'b3e8a998-1cc0-448f-a0d1-61fbfcf88d57')
                    INSERT INTO ExchangeRateSnapshots (Id, UsdToTryRate, EurToTryRate, FetchedAtUtc, Source)
                    VALUES ('b3e8a998-1cc0-448f-a0d1-61fbfcf88d57', 38.6500, 41.9500, '2026-03-31T09:00:00', 'Static seed');
                """);

            migrationBuilder.Sql(
                """
                IF OBJECT_ID(N'MerchantDocumentTypes', N'U') IS NULL
                BEGIN
                    CREATE TABLE MerchantDocumentTypes (
                        Id int NOT NULL,
                        Name varchar(100) NOT NULL,
                        DisplayOrder int NOT NULL,
                        IsRequired bit NOT NULL,
                        CONSTRAINT PK_MerchantDocumentTypes PRIMARY KEY (Id)
                    );
                END
                IF NOT EXISTS (SELECT 1 FROM MerchantDocumentTypes WHERE Id = 1) INSERT INTO MerchantDocumentTypes (Id, Name, DisplayOrder, IsRequired) VALUES (1, 'Vergi Levhasi', 1, 1);
                IF NOT EXISTS (SELECT 1 FROM MerchantDocumentTypes WHERE Id = 2) INSERT INTO MerchantDocumentTypes (Id, Name, DisplayOrder, IsRequired) VALUES (2, 'Imza Sirkuleri', 2, 1);
                IF NOT EXISTS (SELECT 1 FROM MerchantDocumentTypes WHERE Id = 3) INSERT INTO MerchantDocumentTypes (Id, Name, DisplayOrder, IsRequired) VALUES (3, 'Ticaret Sicil Gazetesi', 3, 1);
                IF NOT EXISTS (SELECT 1 FROM MerchantDocumentTypes WHERE Id = 4) INSERT INTO MerchantDocumentTypes (Id, Name, DisplayOrder, IsRequired) VALUES (4, 'Kimlik', 4, 1);
                IF NOT EXISTS (SELECT 1 FROM MerchantDocumentTypes WHERE Id = 5) INSERT INTO MerchantDocumentTypes (Id, Name, DisplayOrder, IsRequired) VALUES (5, 'Uye Isyeri Sozlesmesi', 5, 1);
                """);

            migrationBuilder.Sql(
                """
                IF COL_LENGTH('MerchantDocuments', 'FilePath') IS NOT NULL
                    ALTER TABLE MerchantDocuments DROP COLUMN FilePath;

                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MerchantDocuments_MerchantApplicationId' AND object_id = OBJECT_ID('MerchantDocuments'))
                    CREATE INDEX IX_MerchantDocuments_MerchantApplicationId ON MerchantDocuments(MerchantApplicationId);

                IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_MerchantDocuments_MerchantApplications_MerchantApplicationId')
                    ALTER TABLE MerchantDocuments
                    ADD CONSTRAINT FK_MerchantDocuments_MerchantApplications_MerchantApplicationId
                    FOREIGN KEY (MerchantApplicationId) REFERENCES MerchantApplications(Id) ON DELETE CASCADE;
                """);

            migrationBuilder.Sql(
                """
                DECLARE @dropSql nvarchar(max);
                
                IF COL_LENGTH('MerchantApplications', 'HomePhone') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'HomePhone';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN HomePhone;
                END

                IF COL_LENGTH('MerchantApplications', 'WorkPhone') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'WorkPhone';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN WorkPhone;
                END

                IF COL_LENGTH('MerchantApplications', 'EstimatedMonthlyTurnover') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'EstimatedMonthlyTurnover';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN EstimatedMonthlyTurnover;
                END

                IF COL_LENGTH('MerchantApplications', 'UsdToTryRate') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'UsdToTryRate';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN UsdToTryRate;
                END

                IF COL_LENGTH('MerchantApplications', 'EurToTryRate') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'EurToTryRate';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN EurToTryRate;
                END

                IF COL_LENGTH('MerchantApplications', 'ExchangeRateFetchedAt') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'ExchangeRateFetchedAt';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN ExchangeRateFetchedAt;
                END

                IF COL_LENGTH('MerchantApplications', 'LastStatusChangedAt') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'LastStatusChangedAt';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN LastStatusChangedAt;
                END

                IF COL_LENGTH('MerchantApplications', 'StatusNote') IS NOT NULL
                BEGIN
                    SET @dropSql = NULL;
                    SELECT @dropSql = 'ALTER TABLE MerchantApplications DROP CONSTRAINT [' + dc.name + ']'
                    FROM sys.default_constraints dc
                    INNER JOIN sys.columns c ON c.default_object_id = dc.object_id
                    WHERE dc.parent_object_id = OBJECT_ID('MerchantApplications') AND c.name = 'StatusNote';
                    IF @dropSql IS NOT NULL EXEC (@dropSql);
                    ALTER TABLE MerchantApplications DROP COLUMN StatusNote;
                END
                """);

            migrationBuilder.Sql(
                """
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MerchantApplications_ApplicationNumber' AND object_id = OBJECT_ID('MerchantApplications'))
                    CREATE UNIQUE INDEX IX_MerchantApplications_ApplicationNumber ON MerchantApplications(ApplicationNumber);
                IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_MerchantApplications_TaxNumber' AND object_id = OBJECT_ID('MerchantApplications'))
                    CREATE UNIQUE INDEX IX_MerchantApplications_TaxNumber ON MerchantApplications(TaxNumber);
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Intentionally left empty. This migration reshapes an existing legacy schema,
            // and a full rollback would be destructive for migrated data.
        }
    }
}
