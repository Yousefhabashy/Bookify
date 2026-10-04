using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Bookify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RestrictDeleteBehavior : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_apartments_apartment_id",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "fk_reviews_apartments_apartment_id",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "fk_reviews_bookings_booking_id",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "fk_reviews_users_user_id",
                table: "reviews");

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_apartments_apartment_id",
                table: "bookings",
                column: "apartment_id",
                principalTable: "apartments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_reviews_apartments_apartment_id",
                table: "reviews",
                column: "apartment_id",
                principalTable: "apartments",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_reviews_bookings_booking_id",
                table: "reviews",
                column: "booking_id",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "fk_reviews_users_user_id",
                table: "reviews",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_bookings_apartments_apartment_id",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings");

            migrationBuilder.DropForeignKey(
                name: "fk_reviews_apartments_apartment_id",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "fk_reviews_bookings_booking_id",
                table: "reviews");

            migrationBuilder.DropForeignKey(
                name: "fk_reviews_users_user_id",
                table: "reviews");

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_apartments_apartment_id",
                table: "bookings",
                column: "apartment_id",
                principalTable: "apartments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_bookings_users_user_id",
                table: "bookings",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_reviews_apartments_apartment_id",
                table: "reviews",
                column: "apartment_id",
                principalTable: "apartments",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_reviews_bookings_booking_id",
                table: "reviews",
                column: "booking_id",
                principalTable: "bookings",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "fk_reviews_users_user_id",
                table: "reviews",
                column: "user_id",
                principalTable: "users",
                principalColumn: "id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
