using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AlpineNeeds.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCategorySeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("DELETE FROM Products");
            migrationBuilder.Sql("DELETE FROM Orders");
            migrationBuilder.Sql("DELETE FROM Categories");

            migrationBuilder.InsertData(
                table: "Categories",
                columns: new[] { "Id", "DisplayOrder", "Name", "ParentCategoryId" },
                values: new object[,]
                {
                    { 1, 1, "Облекло", null },
                    { 2, 2, "Обувки / Гети", null },
                    { 3, 3, "Трекинг", null },
                    { 4, 4, "Алпинизъм / Катерене", null },
                    { 5, 1, "Якета мембрана", 1 },
                    { 6, 2, "Якета пух / Primaloft", 1 },
                    { 7, 3, "Якета софтшел", 1 },
                    { 8, 4, "Полари / Флийс", 1 },
                    { 9, 5, "Панталони мембрана", 1 },
                    { 10, 6, "Панталони", 1 },
                    { 11, 7, "Термо бельо", 1 },
                    { 12, 8, "Тениски / Блузи", 1 },
                    { 13, 9, "Чорапи", 1 },
                    { 14, 10, "Ръкавици", 1 },
                    { 15, 11, "Шапки / Шалове", 1 },
                    { 16, 12, "Дъждобрани", 1 },
                    { 17, 13, "Аксесоари", 1 },
                    { 18, 1, "Високи", 2 },
                    { 19, 2, "Средни", 2 },
                    { 20, 3, "Ниски", 2 },
                    { 21, 1, "Осветление", 3 },
                    { 22, 2, "Палатки", 3 },
                    { 23, 3, "Посуда / Примуси", 3 },
                    { 24, 4, "Раници / Чанти", 3 },
                    { 25, 5, "Спални чували", 3 },
                    { 26, 6, "Шалтета / Постелки", 3 },
                    { 27, 7, "Щеки", 3 },
                    { 28, 8, "Снегоходки / Котки", 3 },
                    { 29, 9, "Гети / Дъждобрани", 3 },
                    { 30, 10, "Трекинг аксесоари", 3 },
                    { 31, 1, "Въжета", 4 },
                    { 32, 2, "Седалки", 4 },
                    { 33, 3, "Еспадрили", 4 },
                    { 34, 4, "Карабинери / Примки", 4 },
                    { 35, 5, "Съоръжения от лента", 4 },
                    { 36, 6, "Каски", 4 },
                    { 37, 7, "Клеми / Френдове", 4 },
                    { 38, 8, "Магнезий / Торбички Mg", 4 },
                    { 39, 9, "Осигуряващи съоръжения", 4 },
                    { 40, 10, "Клинове скални / ледени", 4 },
                    { 41, 11, "Ледокопи / Котки", 4 },
                    { 42, 12, "Виа Ферата", 4 },
                    { 43, 13, "Аксесоари", 4 }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
        }
    }
}
