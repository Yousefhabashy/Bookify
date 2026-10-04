using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeAmenitiesToArray : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE apartments 
                ALTER COLUMN amenities TYPE integer[] 
                USING CASE 
                    WHEN amenities = '' THEN '{}'::integer[]
                    WHEN amenities IS NULL THEN NULL
                    ELSE string_to_array(amenities, ',')::integer[]
                END;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                ALTER TABLE apartments 
                ALTER COLUMN amenities TYPE text 
                USING array_to_string(amenities, ',');
            ");
        }
    }
}
