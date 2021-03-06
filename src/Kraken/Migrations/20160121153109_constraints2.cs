using System;
using System.Collections.Generic;
using Microsoft.Data.Entity.Migrations;

namespace Kraken.Migrations
{
    public partial class constraints2 : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ReleaseBatchItem_ReleaseBatch_ReleaseBatchId", schema: "kraken", table: "ReleaseBatchItem");
            migrationBuilder.DropUniqueConstraint(name: "AK_ReleaseBatch_Name", schema: "kraken", table: "ReleaseBatch");
            migrationBuilder.CreateIndex(
                name: "IX_ReleaseBatch_Name",
                schema: "kraken",
                table: "ReleaseBatch",
                column: "Name",
                unique: true);
            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseBatchItem_ReleaseBatch_ReleaseBatchId",
                schema: "kraken",
                table: "ReleaseBatchItem",
                column: "ReleaseBatchId",
                principalSchema: "kraken",
                principalTable: "ReleaseBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(name: "FK_ReleaseBatchItem_ReleaseBatch_ReleaseBatchId", schema: "kraken", table: "ReleaseBatchItem");
            migrationBuilder.DropIndex(name: "IX_ReleaseBatch_Name", schema: "kraken", table: "ReleaseBatch");
            migrationBuilder.AddUniqueConstraint(
                name: "AK_ReleaseBatch_Name",
                schema: "kraken",
                table: "ReleaseBatch",
                column: "Name");
            migrationBuilder.AddForeignKey(
                name: "FK_ReleaseBatchItem_ReleaseBatch_ReleaseBatchId",
                schema: "kraken",
                table: "ReleaseBatchItem",
                column: "ReleaseBatchId",
                principalSchema: "kraken",
                principalTable: "ReleaseBatch",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
