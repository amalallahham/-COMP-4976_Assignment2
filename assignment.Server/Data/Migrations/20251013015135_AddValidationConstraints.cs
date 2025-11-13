using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ObituaryApplication.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddValidationConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add check constraints for data validation
            migrationBuilder.Sql(@"
                CREATE TRIGGER CheckObituaryValidation
                BEFORE INSERT ON Obituaries
                BEGIN
                    SELECT CASE
                        WHEN LENGTH(NEW.FullName) < 1 THEN
                            RAISE(ABORT, 'FullName must be at least 1 character long')
                        WHEN LENGTH(NEW.Biography) < 10 THEN
                            RAISE(ABORT, 'Biography must be at least 10 characters long')
                        WHEN NEW.DOB >= NEW.DOD THEN
                            RAISE(ABORT, 'Date of Birth must be before Date of Death')
                    END;
                END;
            ");

            migrationBuilder.Sql(@"
                CREATE TRIGGER CheckObituaryValidationUpdate
                BEFORE UPDATE ON Obituaries
                BEGIN
                    SELECT CASE
                        WHEN LENGTH(NEW.FullName) < 1 THEN
                            RAISE(ABORT, 'FullName must be at least 1 character long')
                        WHEN LENGTH(NEW.Biography) < 10 THEN
                            RAISE(ABORT, 'Biography must be at least 10 characters long')
                        WHEN NEW.DOB >= NEW.DOD THEN
                            RAISE(ABORT, 'Date of Birth must be before Date of Death')
                    END;
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove validation triggers
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CheckObituaryValidation;");
            migrationBuilder.Sql("DROP TRIGGER IF EXISTS CheckObituaryValidationUpdate;");
        }
    }
}
