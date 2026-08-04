using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SmartEstimate.Infrastructure.Persistence;

#nullable disable

namespace SmartEstimate.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    [DbContext(typeof(SmartEstimateDbContext))]
    [Migration("20260804120000_SeedMvpKnowledgeBase")]
    public partial class SeedMvpKnowledgeBase : Migration
    {
        private const string SeedTimestamp = "2026-08-04T00:00:00Z";

        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var category in Categories)
            {
                migrationBuilder.Sql(UpsertCategorySql(category));
            }

            foreach (var unit in Units)
            {
                migrationBuilder.Sql(UpsertUnitSql(unit));
            }

            foreach (var work in Works)
            {
                migrationBuilder.Sql(UpsertWorkSql(work));
            }

            foreach (var material in Materials)
            {
                migrationBuilder.Sql(UpsertMaterialSql(material));
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(DeleteByIdsSql("ConstructionWorks", Works.Select(work => StableId("work", work.Uk))));
            migrationBuilder.Sql(DeleteByIdsSql("KnowledgeMaterials", Materials.Select(material => StableId("material", material.Uk))));
            migrationBuilder.Sql(DeleteByIdsSql("KnowledgeCategories", Categories.Select(category => StableId("category", category.Key))));
            migrationBuilder.Sql(DeleteByIdsSql("MeasurementUnits", Units.Select(unit => StableId("unit", unit.Key))));
        }

        private static string UpsertCategorySql(CategorySeed category)
        {
            var id = StableId("category", category.Key);
            return $"""
                INSERT INTO "KnowledgeCategories" ("Id", "NameUk", "NameEn", "NameDe", "Description", "ParentCategoryId", "Version", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "Status")
                VALUES ('{id}'::uuid, {Sql(category.Uk)}, {Sql(category.En)}, {Sql(category.De)}, {Sql(category.Description)}, NULL, 1, TIMESTAMPTZ '{SeedTimestamp}', TIMESTAMPTZ '{SeedTimestamp}', NULL, NULL, 'Active')
                ON CONFLICT ("NameUk") DO UPDATE SET
                    "NameEn" = EXCLUDED."NameEn",
                    "NameDe" = EXCLUDED."NameDe",
                    "Description" = EXCLUDED."Description",
                    "Version" = 1,
                    "UpdatedAt" = TIMESTAMPTZ '{SeedTimestamp}',
                    "UpdatedBy" = NULL,
                    "Status" = 'Active';
                """;
        }

        private static string UpsertUnitSql(UnitSeed unit)
        {
            var id = StableId("unit", unit.Key);
            return $"""
                INSERT INTO "MeasurementUnits" ("Id", "Symbol", "NameUk", "NameEn", "NameDe", "Version", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "Status")
                VALUES ('{id}'::uuid, {Sql(unit.Symbol)}, {Sql(unit.Uk)}, {Sql(unit.En)}, {Sql(unit.De)}, 1, TIMESTAMPTZ '{SeedTimestamp}', TIMESTAMPTZ '{SeedTimestamp}', NULL, NULL, 'Active')
                ON CONFLICT ("Symbol") DO UPDATE SET
                    "NameUk" = EXCLUDED."NameUk",
                    "NameEn" = EXCLUDED."NameEn",
                    "NameDe" = EXCLUDED."NameDe",
                    "Version" = 1,
                    "UpdatedAt" = TIMESTAMPTZ '{SeedTimestamp}',
                    "UpdatedBy" = NULL,
                    "Status" = 'Active';
                """;
        }

        private static string UpsertWorkSql(WorkSeed work)
        {
            var id = StableId("work", work.Uk);
            var category = Categories.Single(value => value.Key == work.CategoryKey);
            var unit = Units.Single(value => value.Key == work.UnitKey);
            var description = $"Позиція кошторису: {work.Uk.ToLowerInvariant()}. Застосовується для ремонту квартир і приватних будинків у Києві та Київській області.";
            var tags = Tags(category.Key, work.Tags);
            return $"""
                INSERT INTO "ConstructionWorks" ("Id", "NameUk", "NameEn", "NameDe", "Description", "CategoryId", "UnitId", "Tags", "Version", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "Status")
                SELECT '{id}'::uuid, {Sql(work.Uk)}, {Sql(work.En)}, {Sql(work.De)}, {Sql(description)}, category."Id", unit."Id", {Sql(tags)}, 1, TIMESTAMPTZ '{SeedTimestamp}', TIMESTAMPTZ '{SeedTimestamp}', NULL, NULL, 'Active'
                FROM "KnowledgeCategories" category
                CROSS JOIN "MeasurementUnits" unit
                WHERE category."NameUk" = {Sql(category.Uk)} AND unit."Symbol" = {Sql(unit.Symbol)}
                ON CONFLICT ("NameUk") DO UPDATE SET
                    "NameEn" = EXCLUDED."NameEn",
                    "NameDe" = EXCLUDED."NameDe",
                    "Description" = EXCLUDED."Description",
                    "CategoryId" = EXCLUDED."CategoryId",
                    "UnitId" = EXCLUDED."UnitId",
                    "Tags" = EXCLUDED."Tags",
                    "Version" = 1,
                    "UpdatedAt" = TIMESTAMPTZ '{SeedTimestamp}',
                    "UpdatedBy" = NULL,
                    "Status" = 'Active';
                """;
        }

        private static string UpsertMaterialSql(MaterialSeed material)
        {
            var id = StableId("material", material.Uk);
            var category = Categories.Single(value => value.Key == material.CategoryKey);
            var unit = Units.Single(value => value.Key == material.UnitKey);
            var description = $"Матеріал для розділу «{category.Uk.ToLowerInvariant()}»: {material.Uk}. Використовується у ремонтних роботах квартир і приватних будинків.";
            var tags = Tags(category.Key, material.Tags);
            return $"""
                INSERT INTO "KnowledgeMaterials" ("Id", "NameUk", "NameEn", "NameDe", "Description", "CategoryId", "UnitId", "Tags", "Version", "CreatedAt", "UpdatedAt", "CreatedBy", "UpdatedBy", "Status")
                SELECT '{id}'::uuid, {Sql(material.Uk)}, {Sql(material.En)}, {Sql(material.De)}, {Sql(description)}, category."Id", unit."Id", {Sql(tags)}, 1, TIMESTAMPTZ '{SeedTimestamp}', TIMESTAMPTZ '{SeedTimestamp}', NULL, NULL, 'Active'
                FROM "KnowledgeCategories" category
                CROSS JOIN "MeasurementUnits" unit
                WHERE category."NameUk" = {Sql(category.Uk)} AND unit."Symbol" = {Sql(unit.Symbol)}
                ON CONFLICT ("NameUk") DO UPDATE SET
                    "NameEn" = EXCLUDED."NameEn",
                    "NameDe" = EXCLUDED."NameDe",
                    "Description" = EXCLUDED."Description",
                    "CategoryId" = EXCLUDED."CategoryId",
                    "UnitId" = EXCLUDED."UnitId",
                    "Tags" = EXCLUDED."Tags",
                    "Version" = 1,
                    "UpdatedAt" = TIMESTAMPTZ '{SeedTimestamp}',
                    "UpdatedBy" = NULL,
                    "Status" = 'Active';
                """;
        }

        private static string DeleteByIdsSql(string table, IEnumerable<Guid> ids) =>
            $"""DELETE FROM "{table}" WHERE "Id" IN ({string.Join(", ", ids.Select(id => $"'{id}'::uuid"))});""";

        private static string Tags(string categoryKey, string tags) =>
            string.Join(',', new[] { categoryKey, "kyiv", "renovation" }
                .Concat(tags.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
                .Distinct(StringComparer.OrdinalIgnoreCase));

        private static string Sql(string value) =>
            $"'{value.Replace("'", "''", StringComparison.Ordinal)}'";

        private static Guid StableId(string kind, string key)
        {
            var hash = SHA256.HashData(Encoding.UTF8.GetBytes($"{kind}:{key}"));
            var bytes = hash[..16];
            bytes[6] = (byte)((bytes[6] & 0x0F) | 0x30);
            bytes[8] = (byte)((bytes[8] & 0x3F) | 0x80);
            return new Guid(bytes);
        }

        private static CategorySeed[] Categories => ParseCategories(CategoryData);
        private static UnitSeed[] Units => ParseUnits(UnitData);
        private static WorkSeed[] Works => ParseWorks(WorkData);
        private static MaterialSeed[] Materials => ParseMaterials(MaterialData);

        private static CategorySeed[] ParseCategories(string data) =>
            Lines(data).Select(line =>
            {
                var parts = line.Split("|", StringSplitOptions.TrimEntries);
                return new CategorySeed(parts[0], parts[1], parts[2], parts[3], parts[4]);
            }).ToArray();

        private static UnitSeed[] ParseUnits(string data) =>
            Lines(data).Select(line =>
            {
                var parts = line.Split("|", StringSplitOptions.TrimEntries);
                return new UnitSeed(parts[0], parts[1], parts[2], parts[3], parts[4]);
            }).ToArray();

        private static WorkSeed[] ParseWorks(string data) =>
            Lines(data).Select(line =>
            {
                var parts = line.Split("|", StringSplitOptions.TrimEntries);
                return new WorkSeed(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]);
            }).ToArray();

        private static MaterialSeed[] ParseMaterials(string data) =>
            Lines(data).Select(line =>
            {
                var parts = line.Split("|", StringSplitOptions.TrimEntries);
                return new MaterialSeed(parts[0], parts[1], parts[2], parts[3], parts[4], parts[5]);
            }).ToArray();

        private static IEnumerable<string> Lines(string data) =>
            data.Split("\n", StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
                .Where(line => line[0] != '#');

        private sealed record CategorySeed(string Key, string Uk, string En, string De, string Description);
        private sealed record UnitSeed(string Key, string Symbol, string Uk, string En, string De);
        private sealed record WorkSeed(string CategoryKey, string UnitKey, string Uk, string En, string De, string Tags);
        private sealed record MaterialSeed(string CategoryKey, string UnitKey, string Uk, string En, string De, string Tags);

        private const string CategoryData = """
            demolition|Демонтажні роботи|Demolition works|Abbrucharbeiten|Розбирання старих конструкцій, покриттів, інженерних мереж та підготовка приміщень до ремонту.
            preparation|Підготовчі роботи|Preparatory works|Vorbereitungsarbeiten|Захист, розмітка, тимчасові рішення та базова підготовка об'єкта перед основними роботами.
            foundation|Земляні та фундаментні роботи|Earthworks and foundation works|Erd- und Fundamentarbeiten|Роботи для приватних будинків, терас, прибудов, ганків та локальних бетонних конструкцій.
            masonry|Кладочні роботи|Masonry works|Mauerarbeiten|Зведення перегородок, простінків, ніш та локальних конструкцій з блоків і цегли.
            plaster|Штукатурні роботи|Plastering works|Putzarbeiten|Вирівнювання стін, укосів і стель цементними, гіпсовими та спеціальними сумішами.
            putty|Шпаклювальні роботи|Putty and skim coat works|Spachtelarbeiten|Підготовка поверхонь під фарбування, шпалери та декоративне оздоблення.
            painting|Малярні роботи|Painting works|Malerarbeiten|Ґрунтування, фарбування та декоративне оздоблення внутрішніх поверхонь.
            wallpaper|Поклейка шпалер|Wallpaper works|Tapezierarbeiten|Підготовка та поклейка різних типів шпалер у житлових приміщеннях.
            tiling|Плиткові роботи|Tile works|Fliesenarbeiten|Укладання керамічної плитки, керамограніту, мозаїки та супутні операції.
            flooring|Підлоги|Flooring works|Bodenarbeiten|Стяжки, наливні підлоги, підкладки та монтаж фінішних покриттів.
            drywall|Гіпсокартонні роботи|Drywall works|Trockenbauarbeiten|Каркаси, обшивки, перегородки, ніші та короби з гіпсокартону.
            ceiling|Стелі|Ceiling works|Deckenarbeiten|Підвісні, натяжні, рейкові та декоративні стельові рішення.
            electrical|Електромонтажні роботи|Electrical installation works|Elektroinstallationsarbeiten|Монтаж електропроводки, щитів, ліній, точок живлення та захисних пристроїв.
            lighting|Освітлення|Lighting|Beleuchtung|Монтаж світильників, LED-підсвітки, профілів, керування та декоративного освітлення.
            low_voltage|Слаботочні системи|Low-voltage systems|Schwachstromsysteme|Інтернет, телебачення, домофонія, відеоспостереження та базові smart home лінії.
            plumbing|Сантехнічні роботи|Plumbing works|Sanitärarbeiten|Водопостачання, підключення сантехнічних приладів, колекторні вузли та арматура.
            sewerage|Каналізація|Sewerage|Abwasserinstallation|Монтаж внутрішньої каналізації, трапів, стояків, ревізій і підключень.
            heating|Опалення|Heating|Heizung|Радіатори, тепла підлога, котельні вузли та трубопроводи опалення.
            ventilation|Вентиляція|Ventilation|Lüftung|Побутова вентиляція, витяжки, повітроводи, клапани та санвузлові вентилятори.
            windows|Вікна та скління|Windows and glazing|Fenster und Verglasung|Монтаж, демонтаж, регулювання та оздоблення віконних блоків і скління.
            doors|Двері|Doors|Türen|Монтаж міжкімнатних, вхідних, прихованих і технічних дверей та супутньої фурнітури.
            facade|Фасадні роботи|Facade works|Fassadenarbeiten|Оздоблення, ремонт, утеплення та захист фасадів приватних будинків.
            roofing|Покрівельні роботи|Roofing works|Dacharbeiten|Монтаж та ремонт покрівельних покриттів, водостоків і добірних елементів.
            insulation|Теплоізоляція та звукоізоляція|Thermal and acoustic insulation|Wärme- und Schalldämmung|Ізоляційні роботи для стін, підлог, стель, покрівель і інженерних зон.
            cleaning|Прибирання після ремонту|Post-renovation cleaning|Baureinigung|Фінішне, післябудівельне та спеціалізоване очищення приміщень.
            outdoor|Зовнішній благоустрій|Outdoor improvement|Außenanlagen|Мощення, дренаж, відмостки, тераси, огорожі та локальні роботи біля будинку.
            """;

        private const string UnitData = """
            m2|м²|Квадратний метр|Square metre|Quadratmeter
            m3|м³|Кубічний метр|Cubic metre|Kubikmeter
            m|м|Метр|Metre|Meter
            linear_m|пог. м|Погонний метр|Linear metre|Laufmeter
            piece|шт|Штука|Piece|Stück
            set|комплект|Комплект|Set|Set
            point|точка|Точка|Point|Punkt
            liter|л|Літр|Litre|Liter
            kg|кг|Кілограм|Kilogram|Kilogramm
            ton|т|Тонна|Tonne|Tonne
            bag|мішок|Мішок|Bag|Sack
            roll|рулон|Рулон|Roll|Rolle
            pack|упаковка|Упаковка|Package|Packung
            bucket|відро|Відро|Bucket|Eimer
            sheet|лист|Лист|Sheet|Platte
            slab|плита|Плита|Slab|Platte
            section|секція|Секція|Section|Sektion
            circuit|контур|Контур|Circuit|Kreis
            channel|канал|Канал|Channel|Kanal
            pair|пара|Пара|Pair|Paar
            kw|кВт|Кіловат|Kilowatt|Kilowatt
            hour|год|Година|Hour|Stunde
            day|день|День|Day|Tag
            shift|зміна|Зміна|Shift|Schicht
            trip|рейс|Рейс|Trip|Fahrt
            place|місце|Місце|Place|Stelle
            opening|отвір|Отвір|Opening|Öffnung
            node|вузол|Вузол|Node|Knoten
            device|прилад|Прилад|Device|Gerät
            corner|кут|Кут|Corner|Ecke
            step|сходинка|Сходинка|Step|Stufe
            doorway|проріз|Проріз|Door opening|Durchbruch
            room|кімната|Кімната|Room|Raum
            object|об'єкт|Об'єкт|Object|Objekt
            layer|шар|Шар|Layer|Schicht
            cartridge|картридж|Картридж|Cartridge|Kartusche
            """;

        private const string WorkData = """
            demolition|m2|Демонтаж шпалер зі стін|Removal of wallpaper from walls|Entfernung von Tapeten an Wänden|демонтаж,стіни,шпалери
            demolition|m2|Демонтаж фарби зі стін|Removal of paint from walls|Entfernung von Wandfarbe|демонтаж,стіни,фарба
            demolition|m2|Демонтаж штукатурки зі стін|Removal of wall plaster|Entfernung von Wandputz|демонтаж,стіни,штукатурка
            demolition|m2|Демонтаж керамічної плитки зі стін|Removal of ceramic wall tiles|Entfernung keramischer Wandfliesen|демонтаж,плитка,стіни
            demolition|m2|Демонтаж плитки з підлоги|Removal of floor tiles|Entfernung von Bodenfliesen|демонтаж,плитка,підлога
            demolition|m2|Демонтаж цементної стяжки|Removal of cement screed|Entfernung von Zementestrich|демонтаж,стяжка,підлога
            demolition|m2|Демонтаж ламінату|Removal of laminate flooring|Entfernung von Laminatboden|демонтаж,ламінат,підлога
            demolition|m2|Демонтаж паркету|Removal of parquet flooring|Entfernung von Parkettboden|демонтаж,паркет,підлога
            demolition|m2|Демонтаж лінолеуму|Removal of linoleum flooring|Entfernung von Linoleum|демонтаж,лінолеум,підлога
            demolition|m2|Демонтаж підвісної стелі|Removal of suspended ceiling|Demontage einer abgehängten Decke|демонтаж,стеля
            demolition|m2|Демонтаж гіпсокартонної обшивки|Removal of drywall lining|Demontage von Gipskartonverkleidung|демонтаж,гіпсокартон
            demolition|m2|Демонтаж перегородок з гіпсокартону|Removal of drywall partitions|Demontage von Gipskartonwänden|демонтаж,перегородки
            demolition|m2|Демонтаж цегляних перегородок|Removal of brick partitions|Abbruch von Ziegeltrennwänden|демонтаж,цегла
            demolition|m2|Демонтаж перегородок з газоблоку|Removal of aerated concrete block partitions|Abbruch von Porenbetontrennwänden|демонтаж,газоблок
            demolition|piece|Демонтаж міжкімнатного дверного блоку|Removal of an interior door set|Demontage eines Innentürblocks|демонтаж,двері
            demolition|piece|Демонтаж вхідних дверей|Removal of an entrance door|Demontage einer Eingangstür|демонтаж,двері
            demolition|piece|Демонтаж віконного блоку|Removal of a window unit|Demontage eines Fensterblocks|демонтаж,вікна
            demolition|piece|Демонтаж радіатора опалення|Removal of a heating radiator|Demontage eines Heizkörpers|демонтаж,опалення
            demolition|piece|Демонтаж унітаза|Removal of a toilet|Demontage eines WCs|демонтаж,сантехніка
            demolition|piece|Демонтаж ванни|Removal of a bathtub|Demontage einer Badewanne|демонтаж,сантехніка
            demolition|piece|Демонтаж умивальника|Removal of a washbasin|Demontage eines Waschbeckens|демонтаж,сантехніка
            demolition|linear_m|Демонтаж водопровідних труб|Removal of water supply pipes|Demontage von Wasserleitungen|демонтаж,водопровід
            demolition|linear_m|Демонтаж каналізаційних труб|Removal of sewer pipes|Demontage von Abwasserrohren|демонтаж,каналізація
            demolition|point|Демонтаж електричної точки|Removal of an electrical point|Demontage eines Elektroanschlusses|демонтаж,електрика
            demolition|m3|Завантаження будівельного сміття вручну|Manual loading of construction waste|Manuelles Verladen von Bauschutt|демонтаж,сміття
            demolition|trip|Вивезення будівельного сміття|Removal of construction waste|Abtransport von Bauschutt|демонтаж,сміття,логістика
            preparation|m2|Укриття підлоги захисною плівкою|Covering floors with protective film|Abdecken des Bodens mit Schutzfolie|захист,підлога
            preparation|m2|Укриття меблів захисною плівкою|Covering furniture with protective film|Abdecken von Möbeln mit Schutzfolie|захист,меблі
            preparation|linear_m|Проклеювання малярної стрічки по периметру|Applying masking tape around the perimeter|Abkleben des Umfangs mit Malerband|захист,малярні
            preparation|object|Організація тимчасового освітлення|Temporary lighting setup|Einrichtung temporärer Beleuchtung|підготовка,освітлення
            preparation|object|Організація тимчасового електроживлення|Temporary power setup|Einrichtung temporärer Stromversorgung|підготовка,електрика
            preparation|object|Встановлення тимчасового водопостачання|Temporary water supply setup|Einrichtung temporärer Wasserversorgung|підготовка,сантехніка
            preparation|m2|Очищення поверхні від пилу перед роботами|Dust cleaning before works|Entstaubung vor Arbeiten|підготовка,очищення
            preparation|m2|Знежирення поверхні перед оздобленням|Degreasing surfaces before finishing|Entfetten von Oberflächen vor Ausbauarbeiten|підготовка,очищення
            preparation|m2|Обробка поверхні антисептиком|Antiseptic treatment of surfaces|Antiseptische Oberflächenbehandlung|підготовка,антисептик
            preparation|m2|Гідроізоляційна підготовка мокрих зон|Waterproofing preparation of wet areas|Abdichtungsvorbereitung von Nassbereichen|підготовка,гідроізоляція
            preparation|linear_m|Розмітка рівня чистової підлоги|Marking finished floor level|Markierung der fertigen Fußbodenhöhe|розмітка,підлога
            preparation|linear_m|Розмітка осей перегородок|Marking partition axes|Markierung von Trennwandachsen|розмітка,перегородки
            preparation|point|Розмітка електричних точок|Marking electrical points|Markierung von Elektroanschlüssen|розмітка,електрика
            preparation|point|Розмітка сантехнічних виводів|Marking plumbing outlets|Markierung von Sanitäranschlüssen|розмітка,сантехніка
            preparation|m2|Механічне очищення основи під стяжку|Mechanical cleaning of screed substrate|Mechanische Reinigung des Estrichuntergrunds|підготовка,стяжка
            preparation|m2|Нанесення бетоноконтакту на гладку основу|Applying concrete contact primer to smooth substrate|Auftragen von Betonkontakt auf glatten Untergrund|підготовка,бетоноконтакт
            foundation|m3|Ручна розробка ґрунту під фундамент|Manual excavation for foundation|Manueller Bodenaushub für Fundament|земляні,фундамент
            foundation|m3|Механізована розробка ґрунту під фундамент|Mechanical excavation for foundation|Maschineller Bodenaushub für Fundament|земляні,фундамент
            foundation|m2|Улаштування піщаної подушки|Installation of sand bedding|Herstellung eines Sandpolsters|фундамент,пісок
            foundation|m2|Улаштування щебеневої подушки|Installation of crushed stone bedding|Herstellung eines Schotterpolsters|фундамент,щебінь
            foundation|m2|Ущільнення основи віброплитою|Compaction of base with plate compactor|Verdichtung des Untergrunds mit Rüttelplatte|фундамент,ущільнення
            foundation|m2|Монтаж опалубки стрічкового фундаменту|Formwork installation for strip foundation|Schalung für Streifenfundament montieren|фундамент,опалубка
            foundation|kg|В'язання арматурного каркаса фундаменту|Tying reinforcement cage for foundation|Binden des Bewehrungskorbs für Fundament|фундамент,арматура
            foundation|m3|Бетонування стрічкового фундаменту|Concrete casting of strip foundation|Betonieren eines Streifenfundaments|фундамент,бетон
            foundation|m2|Улаштування плитного фундаменту|Installation of slab foundation|Herstellung einer Bodenplatte|фундамент,плита
            foundation|m2|Гідроізоляція фундаменту бітумною мастикою|Waterproofing foundation with bitumen mastic|Fundamentabdichtung mit Bitumenmastix|фундамент,гідроізоляція
            foundation|linear_m|Монтаж дренажної труби біля фундаменту|Installing drainage pipe near foundation|Montage einer Drainageleitung am Fundament|фундамент,дренаж
            foundation|m2|Утеплення цоколю екструдованим пінополістиролом|Insulating plinth with extruded polystyrene|Dämmung des Sockels mit XPS|фундамент,утеплення
            foundation|m2|Улаштування бетонної відмостки|Installation of concrete blind area|Herstellung einer Beton-Traufstreifenfläche|фундамент,відмостка
            foundation|piece|Бетонування опорної тумби під ганок|Casting concrete support pedestal for porch|Betonieren eines Stützfundaments für Eingangspodest|фундамент,ганок
            masonry|m2|Кладка перегородок з газоблоку 100 мм|Laying 100 mm aerated concrete partitions|Mauerwerk aus 100 mm Porenbetonsteinen|кладка,газоблок
            masonry|m2|Кладка перегородок з газоблоку 150 мм|Laying 150 mm aerated concrete partitions|Mauerwerk aus 150 mm Porenbetonsteinen|кладка,газоблок
            masonry|m2|Кладка перегородок з керамічного блоку|Laying ceramic block partitions|Mauerwerk aus Keramikblöcken|кладка,керамоблок
            masonry|m2|Кладка перегородок з повнотілої цегли|Laying solid brick partitions|Mauerwerk aus Vollziegeln|кладка,цегла
            masonry|m2|Кладка перегородок з пустотілої цегли|Laying hollow brick partitions|Mauerwerk aus Hohlziegeln|кладка,цегла
            masonry|m2|Кладка облицювальної цегли|Laying facing brick|Mauerwerk aus Verblendziegeln|кладка,облицювання
            masonry|linear_m|Армування кладки сіткою|Reinforcing masonry with mesh|Bewehrung des Mauerwerks mit Gitter|кладка,армування
            masonry|linear_m|Улаштування перемичок над прорізами|Installing lintels above openings|Einbau von Stürzen über Öffnungen|кладка,перемички
            masonry|m2|Кладка вентканалів з цегли|Brick masonry for ventilation shafts|Ziegelmauerwerk für Lüftungsschächte|кладка,вентиляція
            masonry|m2|Мурування простінків з газоблоку|Masonry of aerated concrete wall piers|Mauerwerk von Porenbeton-Wandpfeilern|кладка,простінки
            masonry|m2|Закладання дверного прорізу газоблоком|Closing door opening with aerated concrete blocks|Zumauern einer Türöffnung mit Porenbeton|кладка,проріз
            masonry|m2|Закладання ніш цеглою|Closing niches with brick masonry|Zumauern von Nischen mit Ziegeln|кладка,ніші
            masonry|m2|Розширення дверного прорізу в перегородці|Widening a door opening in partition|Verbreiterung einer Türöffnung in Trennwand|кладка,проріз
            masonry|m2|Формування отвору під ревізійний люк|Forming an opening for inspection hatch|Herstellung einer Öffnung für Revisionsklappe|кладка,люк
            plaster|m2|Ґрунтування бетонних стін перед штукатуркою|Priming concrete walls before plastering|Grundierung von Betonwänden vor dem Verputzen|штукатурка,стіни
            plaster|m2|Ґрунтування газобетонних стін перед штукатуркою|Priming aerated concrete walls before plastering|Grundierung von Porenbetonwänden vor dem Verputzen|штукатурка,стіни
            plaster|m2|Монтаж штукатурної сітки на стіни|Installing plaster mesh on walls|Montage von Putzgewebe an Wänden|штукатурка,сітка
            plaster|linear_m|Встановлення штукатурних маяків на стіни|Installing wall plaster screed rails|Setzen von Putzschienen an Wänden|штукатурка,маяки
            plaster|m2|Гіпсова штукатурка стін по маяках|Gypsum plastering of walls by screed rails|Gipsputz an Wänden nach Putzschienen|штукатурка,гіпс
            plaster|m2|Цементно-піщана штукатурка стін по маяках|Cement-sand plastering of walls by screed rails|Zement-Sand-Putz an Wänden nach Putzschienen|штукатурка,цемент
            plaster|m2|Машинна гіпсова штукатурка стін|Machine gypsum plastering of walls|Maschineller Gipsputz an Wänden|штукатурка,машинна
            plaster|m2|Штукатурка стель цементною сумішшю|Cement plastering of ceilings|Zementputz an Decken|штукатурка,стеля
            plaster|linear_m|Штукатурка внутрішніх укосів|Plastering interior reveals|Verputzen innerer Laibungen|штукатурка,укоси
            plaster|linear_m|Штукатурка зовнішніх кутів з профілем|Plastering external corners with corner bead|Verputzen von Außenecken mit Profil|штукатурка,кути
            plaster|linear_m|Встановлення кутника під штукатурку|Installing corner bead for plaster|Montage von Eckschutzprofil für Putz|штукатурка,кутник
            plaster|m2|Вирівнювання стін ремонтною сумішшю|Levelling walls with repair compound|Ausgleichen von Wänden mit Reparaturmörtel|штукатурка,вирівнювання
            plaster|m2|Локальний ремонт штукатурки стін|Local repair of wall plaster|Lokale Reparatur von Wandputz|штукатурка,ремонт
            plaster|linear_m|Заштукатурення штроб після електромонтажу|Plaster filling of chases after electrical works|Verputzen von Schlitzen nach Elektroarbeiten|штукатурка,штроби
            plaster|m2|Затирання штукатурки під подальшу шпаклівку|Rubbing plaster for further puttying|Abreiben von Putz für nachfolgendes Spachteln|штукатурка,фініш
            putty|m2|Ґрунтування стін перед шпаклюванням|Priming walls before puttying|Grundierung von Wänden vor dem Spachteln|шпаклівка,стіни
            putty|m2|Стартове шпаклювання стін|Base puttying of walls|Grundspachtelung von Wänden|шпаклівка,стіни
            putty|m2|Фінішне шпаклювання стін|Finish puttying of walls|Feinspachtelung von Wänden|шпаклівка,стіни
            putty|m2|Шпаклювання стін під фарбування|Puttying walls for painting|Spachteln von Wänden für Anstrich|шпаклівка,фарбування
            putty|m2|Шпаклювання стін під шпалери|Puttying walls for wallpaper|Spachteln von Wänden für Tapeten|шпаклівка,шпалери
            putty|m2|Шпаклювання стелі під фарбування|Puttying ceiling for painting|Spachteln der Decke für Anstrich|шпаклівка,стеля
            putty|linear_m|Шпаклювання внутрішніх кутів|Puttying internal corners|Spachteln von Innenecken|шпаклівка,кути
            putty|linear_m|Шпаклювання зовнішніх кутів|Puttying external corners|Spachteln von Außenecken|шпаклівка,кути
            putty|linear_m|Монтаж малярного кутника під шпаклівку|Installing painter corner bead for putty|Montage von Malereckprofil für Spachtel|шпаклівка,кутник
            putty|linear_m|Армування стиків склохолстом|Reinforcing joints with fiberglass fleece|Armierung von Fugen mit Glasvlies|шпаклівка,армування
            putty|m2|Наклеювання склохолста на стіни|Applying fiberglass fleece to walls|Verkleben von Glasvlies an Wänden|склохолст,стіни
            putty|m2|Наклеювання склохолста на стелю|Applying fiberglass fleece to ceiling|Verkleben von Glasvlies an Decke|склохолст,стеля
            putty|m2|Шліфування шпаклівки стін|Sanding wall putty|Schleifen gespachtelter Wände|шпаклівка,шліфування
            putty|m2|Шліфування шпаклівки стелі|Sanding ceiling putty|Schleifen gespachtelter Decke|шпаклівка,шліфування
            putty|m2|Контрольне ґрунтування після шліфування|Control priming after sanding|Kontrollgrundierung nach dem Schleifen|шпаклівка,ґрунт
            painting|m2|Ґрунтування стін перед фарбуванням|Priming walls before painting|Grundierung von Wänden vor dem Streichen|фарбування,стіни
            painting|m2|Ґрунтування стелі перед фарбуванням|Priming ceiling before painting|Grundierung der Decke vor dem Streichen|фарбування,стеля
            painting|m2|Фарбування стін у два шари|Painting walls in two coats|Streichen von Wänden in zwei Schichten|фарбування,стіни
            painting|m2|Фарбування стелі у два шари|Painting ceiling in two coats|Streichen der Decke in zwei Schichten|фарбування,стеля
            painting|m2|Фарбування укосів|Painting reveals|Streichen von Laibungen|фарбування,укоси
            painting|linear_m|Фарбування плінтуса|Painting skirting boards|Streichen von Sockelleisten|фарбування,плінтус
            painting|m2|Фарбування радіаторів|Painting radiators|Streichen von Heizkörpern|фарбування,радіатори
            painting|m2|Фарбування металевих поверхонь|Painting metal surfaces|Streichen von Metallflächen|фарбування,метал
            painting|m2|Фарбування дерев'яних поверхонь|Painting wooden surfaces|Streichen von Holzflächen|фарбування,дерево
            painting|m2|Нанесення декоративної фарби|Applying decorative paint|Auftragen von Dekorfarbe|фарбування,декор
            painting|m2|Нанесення декоративної штукатурки|Applying decorative plaster|Auftragen von Dekorputz|фарбування,декор
            painting|m2|Лакування дерев'яних поверхонь|Varnishing wooden surfaces|Lackieren von Holzflächen|фарбування,лак
            painting|m2|Фарбування труб опалення|Painting heating pipes|Streichen von Heizungsrohren|фарбування,труби
            painting|m2|Перефарбування стін зі зміною кольору|Repainting walls with colour change|Überstreichen von Wänden mit Farbwechsel|фарбування,ремонт
            painting|m2|Локальне підфарбування після монтажу|Local touch-up painting after installation|Lokale Ausbesserung nach Montagearbeiten|фарбування,ремонт
            wallpaper|m2|Ґрунтування стін перед шпалерами|Priming walls before wallpapering|Grundierung von Wänden vor dem Tapezieren|шпалери,стіни
            wallpaper|m2|Поклейка паперових шпалер|Hanging paper wallpaper|Tapezieren mit Papiertapeten|шпалери,паперові
            wallpaper|m2|Поклейка флізелінових шпалер|Hanging non-woven wallpaper|Tapezieren mit Vliestapeten|шпалери,флізелін
            wallpaper|m2|Поклейка вінілових шпалер|Hanging vinyl wallpaper|Tapezieren mit Vinyltapeten|шпалери,вініл
            wallpaper|m2|Поклейка склошпалер|Hanging glass fibre wallpaper|Tapezieren mit Glasfasertapeten|шпалери,скло
            wallpaper|m2|Поклейка фотошпалер|Hanging photo wallpaper|Tapezieren mit Fototapeten|шпалери,фото
            wallpaper|linear_m|Підрізання шпалер по багету|Trimming wallpaper along moulding|Beschneiden von Tapeten an Leisten|шпалери,підрізка
            wallpaper|linear_m|Підрізання шпалер по плінтусу|Trimming wallpaper along skirting|Beschneiden von Tapeten an Sockelleisten|шпалери,підрізка
            wallpaper|m2|Зняття старих шпалер з розмочуванням|Removing old wallpaper with soaking|Entfernung alter Tapeten durch Einweichen|шпалери,демонтаж
            wallpaper|m2|Поклейка шпалер з підбором малюнка|Hanging wallpaper with pattern matching|Tapezieren mit Musteranpassung|шпалери,малюнок
            wallpaper|m2|Поклейка шпалер на стелю|Hanging wallpaper on ceiling|Tapezieren der Decke|шпалери,стеля
            wallpaper|linear_m|Монтаж декоративного бордюру для шпалер|Installing decorative wallpaper border|Montage einer dekorativen Tapetenbordüre|шпалери,бордюр
            tiling|m2|Ґрунтування основи перед плиткою|Priming substrate before tiling|Grundierung des Untergrunds vor Fliesenarbeiten|плитка,основа
            tiling|m2|Гідроізоляція стін у санвузлі|Waterproofing bathroom walls|Abdichtung von Badwänden|плитка,гідроізоляція
            tiling|m2|Гідроізоляція підлоги у санвузлі|Waterproofing bathroom floor|Abdichtung von Badböden|плитка,гідроізоляція
            tiling|m2|Укладання керамічної плитки на стіни|Installing ceramic tiles on walls|Verlegen keramischer Wandfliesen|плитка,стіни
            tiling|m2|Укладання керамічної плитки на підлогу|Installing ceramic tiles on floor|Verlegen keramischer Bodenfliesen|плитка,підлога
            tiling|m2|Укладання керамограніту на підлогу|Installing porcelain stoneware on floor|Verlegen von Feinsteinzeug auf Boden|плитка,керамограніт
            tiling|m2|Укладання великоформатного керамограніту|Installing large-format porcelain stoneware|Verlegen von großformatigem Feinsteinzeug|плитка,керамограніт
            tiling|m2|Укладання мозаїки|Installing mosaic tiles|Verlegen von Mosaikfliesen|плитка,мозаїка
            tiling|linear_m|Різання плитки по прямій|Straight cutting of tiles|Gerader Fliesenschnitt|плитка,різання
            tiling|opening|Вирізання отвору в плитці|Cutting a hole in tile|Ausschneiden einer Öffnung in Fliese|плитка,отвір
            tiling|linear_m|Запил плитки під 45 градусів|Mitre cutting tiles at 45 degrees|Gehrungsschnitt von Fliesen bei 45 Grad|плитка,запил
            tiling|linear_m|Монтаж плиткового кутика|Installing tile trim|Montage von Fliesenabschlussprofil|плитка,кут
            tiling|m2|Затирання міжплиткових швів цементною фугою|Grouting tile joints with cement grout|Verfugen von Fliesen mit Zementfuge|плитка,фуга
            tiling|m2|Затирання міжплиткових швів епоксидною фугою|Grouting tile joints with epoxy grout|Verfugen von Fliesen mit Epoxidfuge|плитка,фуга
            tiling|linear_m|Герметизація примикань силіконом|Sealing junctions with silicone|Abdichten von Anschlüssen mit Silikon|плитка,силікон
            tiling|m2|Укладання плитки на кухонний фартух|Installing tiles on kitchen backsplash|Verlegen von Fliesen an Küchenrückwand|плитка,кухня
            tiling|step|Облицювання сходинки плиткою|Tiling a stair step|Verkleidung einer Stufe mit Fliesen|плитка,сходи
            tiling|linear_m|Монтаж ревізійного люка під плитку|Installing tileable inspection hatch|Montage einer befliesten Revisionsklappe|плитка,люк
            flooring|m2|Ґрунтування основи під стяжку|Priming substrate for screed|Grundierung des Untergrunds für Estrich|підлога,стяжка
            flooring|m2|Улаштування цементно-піщаної стяжки|Installation of cement-sand screed|Herstellung eines Zement-Sand-Estrichs|підлога,стяжка
            flooring|m2|Улаштування напівсухої стяжки|Installation of semi-dry screed|Herstellung eines halbtrockenen Estrichs|підлога,стяжка
            flooring|m2|Улаштування самовирівнювальної підлоги|Installation of self-levelling floor|Herstellung eines selbstnivellierenden Bodens|підлога,наливна
            flooring|m2|Армування стяжки металевою сіткою|Reinforcing screed with metal mesh|Bewehrung des Estrichs mit Metallgitter|підлога,армування
            flooring|linear_m|Монтаж демпферної стрічки|Installing perimeter expansion tape|Montage von Randdämmstreifen|підлога,демпфер
            flooring|m2|Монтаж підкладки під ламінат|Installing underlay for laminate|Verlegen der Unterlage für Laminat|підлога,підкладка
            flooring|m2|Укладання ламінату прямим способом|Installing laminate in straight layout|Verlegen von Laminat im geraden Verband|підлога,ламінат
            flooring|m2|Укладання ламінату по діагоналі|Installing laminate diagonally|Diagonales Verlegen von Laminat|підлога,ламінат
            flooring|m2|Укладання SPC-плитки|Installing SPC flooring|Verlegen von SPC-Bodenbelag|підлога,spc
            flooring|m2|Укладання паркетної дошки плаваючим способом|Floating installation of engineered wood flooring|Schwimmende Verlegung von Parkettdielen|підлога,паркет
            flooring|m2|Приклеювання паркетної дошки|Gluing engineered wood flooring|Verkleben von Parkettdielen|підлога,паркет
            flooring|m2|Укладання лінолеуму|Installing linoleum|Verlegen von Linoleum|підлога,лінолеум
            flooring|m2|Укладання ковроліну|Installing carpet flooring|Verlegen von Teppichboden|підлога,ковролін
            flooring|linear_m|Монтаж пластикового плінтуса|Installing PVC skirting board|Montage von Kunststoffsockelleisten|підлога,плінтус
            flooring|linear_m|Монтаж МДФ-плінтуса|Installing MDF skirting board|Montage von MDF-Sockelleisten|підлога,плінтус
            flooring|linear_m|Монтаж алюмінієвого порога|Installing aluminium threshold strip|Montage einer Aluminiumschwelle|підлога,поріг
            flooring|m2|Шліфування дерев'яної підлоги|Sanding wooden floor|Schleifen eines Holzbodens|підлога,дерево
            flooring|m2|Лакування дерев'яної підлоги|Varnishing wooden floor|Lackieren eines Holzbodens|підлога,лак
            drywall|m2|Монтаж каркаса перегородки з профілю|Installing metal stud partition frame|Montage eines Metallständerwerks für Trennwand|гіпсокартон,каркас
            drywall|m2|Обшивка перегородки гіпсокартоном в один шар|Single-layer drywall sheathing of partition|Einlagige Beplankung einer Trennwand mit Gipskarton|гіпсокартон,перегородка
            drywall|m2|Обшивка перегородки гіпсокартоном у два шари|Double-layer drywall sheathing of partition|Zweilagige Beplankung einer Trennwand mit Gipskarton|гіпсокартон,перегородка
            drywall|m2|Монтаж пристінного каркаса під гіпсокартон|Installing wall lining frame for drywall|Montage einer Vorsatzschale für Gipskarton|гіпсокартон,стіни
            drywall|m2|Обшивка стін гіпсокартоном|Drywall lining of walls|Verkleidung von Wänden mit Gipskarton|гіпсокартон,стіни
            drywall|m2|Монтаж вологостійкого гіпсокартону|Installing moisture-resistant drywall|Montage von imprägnierten Gipskartonplatten|гіпсокартон,вологостійкий
            drywall|m2|Монтаж вогнестійкого гіпсокартону|Installing fire-resistant drywall|Montage von feuerhemmenden Gipskartonplatten|гіпсокартон,вогнестійкий
            drywall|linear_m|Монтаж короба з гіпсокартону|Installing drywall box|Montage eines Gipskartonkastens|гіпсокартон,короб
            drywall|linear_m|Монтаж ніші з гіпсокартону|Installing drywall niche|Herstellung einer Gipskartonnische|гіпсокартон,ніша
            drywall|linear_m|Монтаж відкосу з гіпсокартону|Installing drywall reveal|Montage einer Gipskartonlaibung|гіпсокартон,укіс
            drywall|linear_m|Армування стиків гіпсокартону стрічкою|Reinforcing drywall joints with tape|Armierung von Gipskartonfugen mit Band|гіпсокартон,стики
            drywall|linear_m|Шпаклювання стиків гіпсокартону|Puttying drywall joints|Spachteln von Gipskartonfugen|гіпсокартон,стики
            drywall|opening|Влаштування отвору в гіпсокартонній перегородці|Making an opening in drywall partition|Herstellung einer Öffnung in Gipskartonwand|гіпсокартон,проріз
            drywall|piece|Монтаж ревізійного люка в гіпсокартоні|Installing inspection hatch in drywall|Einbau einer Revisionsklappe in Gipskarton|гіпсокартон,люк
            drywall|m2|Заповнення перегородки мінеральною ватою|Filling partition with mineral wool|Füllen der Trennwand mit Mineralwolle|гіпсокартон,ізоляція
            ceiling|m2|Монтаж однорівневої стелі з гіпсокартону|Installing single-level drywall ceiling|Montage einer einlagigen Gipskartondecke|стеля,гіпсокартон
            ceiling|m2|Монтаж дворівневої стелі з гіпсокартону|Installing two-level drywall ceiling|Montage einer zweistufigen Gipskartondecke|стеля,гіпсокартон
            ceiling|m2|Монтаж натяжної стелі ПВХ|Installing PVC stretch ceiling|Montage einer PVC-Spanndecke|стеля,натяжна
            ceiling|m2|Монтаж тканинної натяжної стелі|Installing fabric stretch ceiling|Montage einer Stoffspanndecke|стеля,натяжна
            ceiling|m2|Монтаж касетної підвісної стелі|Installing cassette suspended ceiling|Montage einer Rasterdecke|стеля,касетна
            ceiling|m2|Монтаж рейкової стелі|Installing slatted ceiling|Montage einer Lamellendecke|стеля,рейкова
            ceiling|linear_m|Монтаж тіньового профілю стелі|Installing shadow gap ceiling profile|Montage eines Schattenfugenprofils|стеля,профіль
            ceiling|linear_m|Монтаж стельового багета|Installing ceiling moulding|Montage einer Deckenleiste|стеля,багет
            ceiling|piece|Встановлення платформи під світильник у натяжній стелі|Installing light fixture platform in stretch ceiling|Montage einer Leuchtenplattform in Spanndecke|стеля,освітлення
            ceiling|piece|Вирізання отвору під світильник у стелі|Cutting ceiling opening for light fixture|Ausschneiden einer Deckenöffnung für Leuchte|стеля,отвір
            ceiling|m2|Шпаклювання гіпсокартонної стелі|Puttying drywall ceiling|Spachteln einer Gipskartondecke|стеля,шпаклівка
            ceiling|m2|Фарбування гіпсокартонної стелі|Painting drywall ceiling|Streichen einer Gipskartondecke|стеля,фарбування
            electrical|point|Монтаж підрозетника в бетоні|Installing socket box in concrete|Montage einer Unterputzdose in Beton|електрика,підрозетник
            electrical|point|Монтаж підрозетника в цеглі|Installing socket box in brick|Montage einer Unterputzdose in Ziegel|електрика,підрозетник
            electrical|point|Монтаж підрозетника в гіпсокартоні|Installing socket box in drywall|Montage einer Hohlwanddose in Gipskarton|електрика,підрозетник
            electrical|linear_m|Штроблення стіни під кабель у бетоні|Chasing concrete wall for cable|Schlitzen einer Betonwand für Kabel|електрика,штроба
            electrical|linear_m|Штроблення стіни під кабель у цеглі|Chasing brick wall for cable|Schlitzen einer Ziegelwand für Kabel|електрика,штроба
            electrical|linear_m|Прокладання кабелю в гофротрубі|Laying cable in corrugated conduit|Verlegen von Kabel in Wellrohr|електрика,кабель
            electrical|linear_m|Прокладання кабелю у штробі|Laying cable in chase|Verlegen von Kabel im Schlitz|електрика,кабель
            electrical|linear_m|Прокладання кабелю по стелі|Laying cable on ceiling|Verlegen von Kabel an der Decke|електрика,кабель
            electrical|point|Монтаж розетки|Installing power socket|Montage einer Steckdose|електрика,розетка
            electrical|point|Монтаж вимикача|Installing switch|Montage eines Schalters|електрика,вимикач
            electrical|point|Монтаж виводу під електроплиту|Installing outlet for electric stove|Montage eines Anschlusses für Elektroherd|електрика,плита
            electrical|point|Монтаж виводу під кондиціонер|Installing outlet for air conditioner|Montage eines Anschlusses für Klimagerät|електрика,кондиціонер
            electrical|point|Монтаж виводу під бойлер|Installing outlet for boiler|Montage eines Anschlusses für Boiler|електрика,бойлер
            electrical|piece|Монтаж квартирного електрощита|Installing apartment distribution board|Montage eines Wohnungsverteilers|електрика,щит
            electrical|piece|Монтаж автоматичного вимикача|Installing circuit breaker|Montage eines Leitungsschutzschalters|електрика,автомат
            electrical|piece|Монтаж диференційного автомата|Installing RCBO|Montage eines FI/LS-Schalters|електрика,захист
            electrical|piece|Монтаж реле напруги|Installing voltage relay|Montage eines Spannungsrelais|електрика,захист
            electrical|point|Підключення електричної теплої підлоги|Connecting electric underfloor heating|Anschluss einer elektrischen Fußbodenheizung|електрика,тепла підлога
            electrical|point|Продзвонювання та маркування кабельних ліній|Testing and labelling cable lines|Prüfen und Beschriften von Kabelkreisen|електрика,тестування
            electrical|point|Монтаж заземлювального провідника|Installing grounding conductor|Verlegen eines Schutzleiters|електрика,заземлення
            lighting|point|Монтаж точкового світильника|Installing recessed spotlight|Montage eines Einbaustrahlers|освітлення,світильник
            lighting|piece|Монтаж накладного світильника|Installing surface-mounted light|Montage einer Aufbauleuchte|освітлення,світильник
            lighting|piece|Монтаж люстри|Installing chandelier|Montage eines Kronleuchters|освітлення,люстра
            lighting|linear_m|Монтаж LED-стрічки|Installing LED strip|Montage eines LED-Streifens|освітлення,led
            lighting|linear_m|Монтаж алюмінієвого профілю для LED-стрічки|Installing aluminium profile for LED strip|Montage eines Aluminiumprofils für LED-Streifen|освітлення,led
            lighting|piece|Монтаж блока живлення LED|Installing LED power supply|Montage eines LED-Netzteils|освітлення,led
            lighting|piece|Монтаж димера|Installing dimmer|Montage eines Dimmers|освітлення,керування
            lighting|piece|Монтаж датчика руху для освітлення|Installing motion sensor for lighting|Montage eines Bewegungsmelders für Beleuchtung|освітлення,датчик
            lighting|piece|Монтаж трекової шини|Installing track light rail|Montage einer Stromschiene|освітлення,трек
            lighting|piece|Монтаж трекового світильника|Installing track light fixture|Montage einer Schienenleuchte|освітлення,трек
            lighting|point|Підключення дзеркала з підсвіткою|Connecting illuminated mirror|Anschluss eines beleuchteten Spiegels|освітлення,ванна
            lighting|linear_m|Монтаж прихованої підсвітки у ніші|Installing concealed niche lighting|Montage verdeckter Nischenbeleuchtung|освітлення,декор
            low_voltage|point|Монтаж інтернет-розетки|Installing data outlet|Montage einer Netzwerkdose|слаботочка,інтернет
            low_voltage|linear_m|Прокладання кабелю UTP|Laying UTP cable|Verlegen von UTP-Kabel|слаботочка,інтернет
            low_voltage|point|Монтаж телевізійної розетки|Installing TV outlet|Montage einer TV-Dose|слаботочка,тв
            low_voltage|linear_m|Прокладання коаксіального кабелю|Laying coaxial cable|Verlegen von Koaxialkabel|слаботочка,тв
            low_voltage|point|Монтаж домофонної точки|Installing intercom point|Montage eines Gegensprechanschlusses|слаботочка,домофон
            low_voltage|point|Монтаж точки відеоспостереження|Installing CCTV point|Montage eines Videoüberwachungspunktes|слаботочка,відео
            low_voltage|piece|Монтаж IP-камери|Installing IP camera|Montage einer IP-Kamera|слаботочка,камера
            low_voltage|piece|Монтаж Wi-Fi точки доступу|Installing Wi-Fi access point|Montage eines WLAN-Access-Points|слаботочка,wifi
            low_voltage|piece|Монтаж слабкострумового щита|Installing low-voltage cabinet|Montage eines Schwachstromverteilers|слаботочка,щит
            low_voltage|point|Підключення датчика протікання|Connecting leak sensor|Anschluss eines Leckagesensors|слаботочка,датчик
            low_voltage|point|Підключення датчика відкривання|Connecting opening sensor|Anschluss eines Öffnungssensors|слаботочка,датчик
            low_voltage|piece|Маркування слабкострумових ліній|Labelling low-voltage lines|Beschriftung von Schwachstromleitungen|слаботочка,маркування
            plumbing|point|Монтаж точки холодної води|Installing cold water point|Montage eines Kaltwasseranschlusses|сантехніка,водопостачання
            plumbing|point|Монтаж точки гарячої води|Installing hot water point|Montage eines Warmwasseranschlusses|сантехніка,водопостачання
            plumbing|linear_m|Прокладання труби PPR для водопостачання|Laying PPR water supply pipe|Verlegen von PPR-Wasserleitung|сантехніка,труби
            plumbing|linear_m|Прокладання труби PEX для водопостачання|Laying PEX water supply pipe|Verlegen von PEX-Wasserleitung|сантехніка,труби
            plumbing|node|Монтаж колекторного вузла водопостачання|Installing water supply manifold unit|Montage einer Wasserverteilergruppe|сантехніка,колектор
            plumbing|piece|Монтаж фільтра грубого очищення|Installing coarse water filter|Montage eines Grobfilters|сантехніка,фільтр
            plumbing|piece|Монтаж редуктора тиску|Installing pressure reducing valve|Montage eines Druckminderers|сантехніка,арматура
            plumbing|piece|Монтаж лічильника води|Installing water meter|Montage eines Wasserzählers|сантехніка,лічильник
            plumbing|piece|Монтаж інсталяції унітаза|Installing concealed toilet frame|Montage eines WC-Vorwandelements|сантехніка,унітаз
            plumbing|piece|Монтаж унітаза|Installing toilet|Montage eines WCs|сантехніка,унітаз
            plumbing|piece|Монтаж умивальника|Installing washbasin|Montage eines Waschbeckens|сантехніка,умивальник
            plumbing|piece|Монтаж змішувача|Installing mixer tap|Montage einer Mischbatterie|сантехніка,змішувач
            plumbing|piece|Монтаж ванни|Installing bathtub|Montage einer Badewanne|сантехніка,ванна
            plumbing|piece|Монтаж душового піддона|Installing shower tray|Montage einer Duschwanne|сантехніка,душ
            plumbing|piece|Монтаж душової системи|Installing shower system|Montage eines Duschsystems|сантехніка,душ
            plumbing|piece|Підключення пральної машини|Connecting washing machine|Anschluss einer Waschmaschine|сантехніка,техніка
            sewerage|linear_m|Прокладання каналізаційної труби 50 мм|Laying 50 mm sewer pipe|Verlegen eines 50-mm-Abwasserrohrs|каналізація,труби
            sewerage|linear_m|Прокладання каналізаційної труби 110 мм|Laying 110 mm sewer pipe|Verlegen eines 110-mm-Abwasserrohrs|каналізація,труби
            sewerage|point|Монтаж каналізаційного виводу під умивальник|Installing sewer outlet for washbasin|Montage eines Abwasseranschlusses für Waschbecken|каналізація,вивід
            sewerage|point|Монтаж каналізаційного виводу під унітаз|Installing sewer outlet for toilet|Montage eines Abwasseranschlusses für WC|каналізація,вивід
            sewerage|point|Монтаж каналізаційного виводу під душ|Installing sewer outlet for shower|Montage eines Abwasseranschlusses für Dusche|каналізація,вивід
            sewerage|piece|Монтаж трапа в душовій зоні|Installing floor drain in shower area|Montage eines Bodenablaufs im Duschbereich|каналізація,трап
            sewerage|piece|Монтаж ревізії каналізації|Installing sewer inspection fitting|Montage einer Abwasserrevision|каналізація,ревізія
            sewerage|linear_m|Шумоізоляція каналізаційної труби|Sound insulation of sewer pipe|Schalldämmung eines Abwasserrohrs|каналізація,шумоізоляція
            sewerage|piece|Підключення сифона умивальника|Connecting washbasin siphon|Anschluss eines Waschbeckensiphons|каналізація,сифон
            sewerage|piece|Підключення сифона ванни|Connecting bathtub siphon|Anschluss eines Badewannensiphons|каналізація,сифон
            sewerage|piece|Підключення сифона кухонної мийки|Connecting kitchen sink siphon|Anschluss eines Spülensiphons|каналізація,сифон
            sewerage|linear_m|Перенесення стояка каналізації у межах санвузла|Relocating sewer riser within bathroom|Versetzen einer Abwassersteigleitung im Bad|каналізація,стояк
            heating|piece|Демонтаж старого радіатора опалення|Removal of old heating radiator|Demontage eines alten Heizkörpers|опалення,радіатор
            heating|piece|Монтаж сталевого панельного радіатора|Installing steel panel radiator|Montage eines Stahlplattenheizkörpers|опалення,радіатор
            heating|piece|Монтаж біметалевого радіатора|Installing bimetal radiator|Montage eines Bimetallheizkörpers|опалення,радіатор
            heating|piece|Монтаж термостатичного клапана|Installing thermostatic valve|Montage eines Thermostatventils|опалення,клапан
            heating|piece|Монтаж крана Маєвського|Installing Mayevsky air vent|Montage eines Entlüftungsventils|опалення,кран
            heating|linear_m|Прокладання труби опалення PEX|Laying PEX heating pipe|Verlegen von PEX-Heizungsrohr|опалення,труби
            heating|linear_m|Прокладання труби опалення PPR|Laying PPR heating pipe|Verlegen von PPR-Heizungsrohr|опалення,труби
            heating|m2|Монтаж водяної теплої підлоги|Installing hydronic underfloor heating|Montage einer Warmwasser-Fußbodenheizung|опалення,тепла підлога
            heating|circuit|Монтаж контуру теплої підлоги|Installing underfloor heating circuit|Montage eines Fußbodenheizkreises|опалення,контур
            heating|node|Монтаж колектора теплої підлоги|Installing underfloor heating manifold|Montage eines Fußbodenheizungsverteilers|опалення,колектор
            heating|node|Монтаж насосно-змішувального вузла|Installing pump mixing unit|Montage einer Pumpenmischgruppe|опалення,вузол
            heating|piece|Монтаж електричного бойлера опалення|Installing electric heating boiler|Montage eines Elektroheizkessels|опалення,котел
            heating|piece|Промивання радіатора опалення|Flushing heating radiator|Spülen eines Heizkörpers|опалення,обслуговування
            heating|circuit|Опресування системи опалення|Pressure testing heating system|Druckprüfung der Heizungsanlage|опалення,випробування
            ventilation|piece|Монтаж побутового витяжного вентилятора|Installing domestic exhaust fan|Montage eines Haushalts-Abluftventilators|вентиляція,вентилятор
            ventilation|piece|Монтаж зворотного клапана вентиляції|Installing ventilation backdraft damper|Montage einer Rückstauklappe für Lüftung|вентиляція,клапан
            ventilation|linear_m|Монтаж пластикового повітроводу|Installing plastic air duct|Montage eines Kunststoff-Luftkanals|вентиляція,повітровід
            ventilation|linear_m|Монтаж гнучкого повітроводу|Installing flexible air duct|Montage eines flexiblen Luftkanals|вентиляція,повітровід
            ventilation|piece|Монтаж вентиляційної решітки|Installing ventilation grille|Montage eines Lüftungsgitters|вентиляція,решітка
            ventilation|piece|Підключення кухонної витяжки|Connecting kitchen hood|Anschluss einer Dunstabzugshaube|вентиляція,кухня
            ventilation|piece|Монтаж припливного клапана у стіні|Installing wall air inlet valve|Montage eines Wand-Zuluftventils|вентиляція,приплив
            ventilation|opening|Буріння отвору під вентиляційний канал|Drilling opening for ventilation duct|Kernbohrung für Lüftungskanal|вентиляція,отвір
            ventilation|linear_m|Теплоізоляція вентиляційного каналу|Thermal insulation of ventilation duct|Wärmedämmung eines Lüftungskanals|вентиляція,ізоляція
            ventilation|piece|Монтаж декоративної вентиляційної накладки|Installing decorative ventilation cover|Montage einer dekorativen Lüftungsabdeckung|вентиляція,накладка
            windows|piece|Демонтаж старого вікна|Removal of old window|Demontage eines alten Fensters|вікна,демонтаж
            windows|piece|Монтаж металопластикового вікна|Installing PVC window|Montage eines Kunststofffensters|вікна,монтаж
            windows|piece|Монтаж алюмінієвого вікна|Installing aluminium window|Montage eines Aluminiumfensters|вікна,монтаж
            windows|linear_m|Монтаж підвіконня|Installing window sill|Montage einer Fensterbank|вікна,підвіконня
            windows|linear_m|Монтаж зовнішнього відливу|Installing exterior window sill|Montage einer Außenfensterbank|вікна,відлив
            windows|linear_m|Монтаж внутрішніх укосів з сендвіч-панелі|Installing interior sandwich panel reveals|Montage innerer Laibungen aus Sandwichpaneel|вікна,укоси
            windows|linear_m|Штукатурка віконних укосів|Plastering window reveals|Verputzen von Fensterlaibungen|вікна,укоси
            windows|linear_m|Шпаклювання віконних укосів|Puttying window reveals|Spachteln von Fensterlaibungen|вікна,укоси
            windows|linear_m|Фарбування віконних укосів|Painting window reveals|Streichen von Fensterlaibungen|вікна,укоси
            windows|piece|Регулювання віконної фурнітури|Adjusting window hardware|Einstellung von Fensterbeschlägen|вікна,фурнітура
            windows|linear_m|Герметизація примикання вікна|Sealing window junction|Abdichten des Fensteranschlusses|вікна,герметик
            windows|piece|Монтаж москітної сітки|Installing mosquito screen|Montage eines Insektenschutzgitters|вікна,сітка
            doors|piece|Демонтаж міжкімнатних дверей|Removal of interior door|Demontage einer Innentür|двері,демонтаж
            doors|piece|Монтаж міжкімнатного дверного блоку|Installing interior door set|Montage eines Innentürblocks|двері,міжкімнатні
            doors|piece|Монтаж прихованих дверей|Installing hidden door|Montage einer verdeckten Tür|двері,приховані
            doors|piece|Монтаж вхідних металевих дверей|Installing metal entrance door|Montage einer Metall-Eingangstür|двері,вхідні
            doors|piece|Монтаж розсувної дверної системи|Installing sliding door system|Montage eines Schiebetürsystems|двері,розсувні
            doors|linear_m|Монтаж дверної лиштви|Installing door casing|Montage von Türbekleidung|двері,лиштва
            doors|piece|Врізання дверного замка|Mortising door lock|Einfräsen eines Türschlosses|двері,замок
            doors|piece|Врізання дверних петель|Mortising door hinges|Einfräsen von Türbändern|двері,петлі
            doors|piece|Монтаж дверної ручки|Installing door handle|Montage eines Türgriffs|двері,ручка
            doors|linear_m|Ущільнення дверної коробки піною|Foam sealing of door frame|Ausschäumen einer Türzarge|двері,піна
            doors|doorway|Розширення дверного прорізу|Widening door opening|Verbreiterung einer Türöffnung|двері,проріз
            doors|doorway|Підготовка прорізу під дверний блок|Preparing opening for door set|Vorbereitung der Öffnung für Türblock|двері,проріз
            facade|m2|Очищення фасаду від старого покриття|Cleaning facade from old coating|Reinigung der Fassade von Altbeschichtung|фасад,очищення
            facade|m2|Ґрунтування фасаду|Priming facade|Grundierung der Fassade|фасад,ґрунт
            facade|m2|Штукатурка фасаду цементною сумішшю|Cement plastering of facade|Zementputz an der Fassade|фасад,штукатурка
            facade|m2|Утеплення фасаду пінополістиролом|Insulating facade with EPS|Fassadendämmung mit EPS|фасад,утеплення
            facade|m2|Утеплення фасаду мінеральною ватою|Insulating facade with mineral wool|Fassadendämmung mit Mineralwolle|фасад,утеплення
            facade|m2|Армування фасаду склосіткою|Reinforcing facade with fiberglass mesh|Armierung der Fassade mit Glasfasergewebe|фасад,армування
            facade|m2|Нанесення декоративної фасадної штукатурки|Applying decorative facade plaster|Auftragen von dekorativem Fassadenputz|фасад,декор
            facade|m2|Фарбування фасаду|Painting facade|Streichen der Fassade|фасад,фарбування
            facade|linear_m|Монтаж фасадного кутника|Installing facade corner profile|Montage eines Fassadeneckprofils|фасад,кутник
            facade|linear_m|Монтаж цокольного профілю|Installing plinth profile|Montage eines Sockelprofils|фасад,профіль
            facade|m2|Облицювання цоколю плиткою|Tiling plinth|Verkleidung des Sockels mit Fliesen|фасад,цоколь
            facade|m2|Монтаж фасадних панелей|Installing facade panels|Montage von Fassadenpaneelen|фасад,панелі
            roofing|m2|Демонтаж старого покрівельного покриття|Removal of old roof covering|Demontage alter Dacheindeckung|покрівля,демонтаж
            roofing|m2|Монтаж кроквяної системи|Installing rafter system|Montage des Sparrensystems|покрівля,крокви
            roofing|m2|Монтаж пароізоляції покрівлі|Installing roof vapour barrier|Montage der Dachdampfsperre|покрівля,пароізоляція
            roofing|m2|Монтаж гідроізоляційної мембрани покрівлі|Installing roof waterproofing membrane|Montage der Dachabdichtungsbahn|покрівля,мембрана
            roofing|m2|Монтаж обрешітки покрівлі|Installing roof battens|Montage der Dachlattung|покрівля,обрешітка
            roofing|m2|Монтаж металочерепиці|Installing metal roof tiles|Montage von Metallziegeln|покрівля,металочерепиця
            roofing|m2|Монтаж бітумної черепиці|Installing bitumen shingles|Verlegen von Bitumenschindeln|покрівля,черепиця
            roofing|m2|Монтаж профнастилу на покрівлю|Installing corrugated roofing sheet|Montage von Trapezblech auf Dach|покрівля,профнастил
            roofing|linear_m|Монтаж конькового елемента|Installing ridge element|Montage des Firstelements|покрівля,коньок
            roofing|linear_m|Монтаж карнизної планки|Installing eaves flashing|Montage der Traufleiste|покрівля,планка
            roofing|linear_m|Монтаж водостічного жолоба|Installing gutter|Montage einer Dachrinne|покрівля,водостік
            roofing|linear_m|Монтаж водостічної труби|Installing downpipe|Montage eines Fallrohrs|покрівля,водостік
            roofing|piece|Монтаж мансардного вікна|Installing roof window|Montage eines Dachfensters|покрівля,вікно
            roofing|m2|Утеплення покрівлі мінеральною ватою|Insulating roof with mineral wool|Dachdämmung mit Mineralwolle|покрівля,утеплення
            insulation|m2|Утеплення стін мінеральною ватою|Insulating walls with mineral wool|Wanddämmung mit Mineralwolle|ізоляція,стіни
            insulation|m2|Утеплення стін екструдованим пінополістиролом|Insulating walls with XPS|Wanddämmung mit XPS|ізоляція,стіни
            insulation|m2|Утеплення підлоги мінеральною ватою|Insulating floor with mineral wool|Bodendämmung mit Mineralwolle|ізоляція,підлога
            insulation|m2|Утеплення підлоги екструдованим пінополістиролом|Insulating floor with XPS|Bodendämmung mit XPS|ізоляція,підлога
            insulation|m2|Звукоізоляція перегородки мінеральною ватою|Sound insulating partition with mineral wool|Schalldämmung einer Trennwand mit Mineralwolle|ізоляція,шумоізоляція
            insulation|m2|Звукоізоляція стелі акустичними плитами|Sound insulating ceiling with acoustic slabs|Deckenschalldämmung mit Akustikplatten|ізоляція,стеля
            insulation|m2|Монтаж пароізоляційної мембрани|Installing vapour barrier membrane|Montage einer Dampfsperrbahn|ізоляція,пароізоляція
            insulation|m2|Монтаж гідроізоляційної мембрани|Installing waterproofing membrane|Montage einer Abdichtungsbahn|ізоляція,гідроізоляція
            insulation|linear_m|Утеплення труб водопостачання|Insulating water supply pipes|Dämmung von Wasserleitungen|ізоляція,труби
            insulation|linear_m|Утеплення труб опалення|Insulating heating pipes|Dämmung von Heizungsrohren|ізоляція,труби
            cleaning|m2|Сухе прибирання приміщення після ремонту|Dry cleaning after renovation|Trockenreinigung nach Renovierung|прибирання,сухе
            cleaning|m2|Вологе прибирання приміщення після ремонту|Wet cleaning after renovation|Nassreinigung nach Renovierung|прибирання,вологе
            cleaning|m2|Знепилення стін та стелі|Dust removal from walls and ceiling|Entstaubung von Wänden und Decken|прибирання,пил
            cleaning|m2|Миття підлоги після ремонту|Washing floors after renovation|Reinigung von Böden nach Renovierung|прибирання,підлога
            cleaning|piece|Миття вікна після ремонту|Cleaning window after renovation|Fensterreinigung nach Renovierung|прибирання,вікна
            cleaning|linear_m|Очищення плінтуса після ремонту|Cleaning skirting after renovation|Reinigung von Sockelleisten nach Renovierung|прибирання,плінтус
            cleaning|m2|Очищення плитки від залишків фуги|Cleaning tiles from grout residue|Entfernung von Fugenresten auf Fliesen|прибирання,плитка
            cleaning|m2|Видалення захисної плівки з поверхонь|Removing protective film from surfaces|Entfernung von Schutzfolie von Oberflächen|прибирання,плівка
            cleaning|m3|Збирання та пакування будівельного сміття|Collecting and bagging construction waste|Sammeln und Verpacken von Bauschutt|прибирання,сміття
            cleaning|object|Фінальне прибирання квартири під здачу|Final apartment cleaning for handover|Endreinigung einer Wohnung zur Übergabe|прибирання,здача
            outdoor|m2|Підготовка основи під тротуарну плитку|Preparing base for paving slabs|Vorbereitung des Untergrunds für Pflastersteine|благоустрій,плитка
            outdoor|m2|Улаштування піщано-щебеневої основи|Installing sand-crushed stone base|Herstellung eines Sand-Schotter-Unterbaus|благоустрій,основа
            outdoor|m2|Укладання тротуарної плитки|Laying paving slabs|Verlegen von Pflastersteinen|благоустрій,плитка
            outdoor|linear_m|Монтаж бордюру|Installing kerbstone|Montage eines Bordsteins|благоустрій,бордюр
            outdoor|linear_m|Монтаж лотка водовідведення|Installing drainage channel|Montage einer Entwässerungsrinne|благоустрій,дренаж
            outdoor|linear_m|Монтаж дренажної труби на ділянці|Installing site drainage pipe|Verlegen einer Grundstücksdrainageleitung|благоустрій,дренаж
            outdoor|m2|Улаштування терасної дошки|Installing decking boards|Montage von Terrassendielen|благоустрій,тераса
            outdoor|linear_m|Монтаж паркану з металопрофілю|Installing metal profile fence|Montage eines Zauns aus Profilblech|благоустрій,паркан
            outdoor|piece|Монтаж хвіртки|Installing pedestrian gate|Montage einer Gartenpforte|благоустрій,ворота
            outdoor|piece|Монтаж воріт|Installing vehicle gate|Montage eines Tores|благоустрій,ворота
            preparation|object|Приймання об'єкта перед початком ремонту|Site handover inspection before renovation|Objektaufnahme vor Renovierungsbeginn|підготовка,приймання
            preparation|room|Фотофіксація стану приміщення до ремонту|Photo documentation of room condition before renovation|Fotodokumentation des Raumzustands vor Renovierung|підготовка,фотофіксація
            preparation|room|Заміри приміщення перед складанням кошторису|Room measurement before estimate preparation|Raumaufmaß vor Kostenvoranschlag|підготовка,заміри
            preparation|object|Складання переліку прихованих робіт|Preparing hidden works checklist|Erstellung einer Liste verdeckter Arbeiten|підготовка,контроль
            demolition|linear_m|Демонтаж старого плінтуса|Removal of old skirting board|Demontage alter Sockelleisten|демонтаж,плінтус
            demolition|m2|Демонтаж старої гідроізоляції|Removal of old waterproofing|Entfernung alter Abdichtung|демонтаж,гідроізоляція
            demolition|piece|Демонтаж кухонної мийки|Removal of kitchen sink|Demontage einer Küchenspüle|демонтаж,кухня
            demolition|piece|Демонтаж душової кабіни|Removal of shower cabin|Demontage einer Duschkabine|демонтаж,душ
            plaster|linear_m|Штукатурка дверних укосів|Plastering door reveals|Verputzen von Türlaibungen|штукатурка,двері
            plaster|m2|Штукатурка санвузла цементною сумішшю|Cement plastering of bathroom walls|Zementputz im Badezimmer|штукатурка,санвузол
            plaster|m2|Вирівнювання стін під великоформатну плитку|Levelling walls for large-format tiles|Ausgleichen von Wänden für großformatige Fliesen|штукатурка,плитка
            plaster|linear_m|Формування прямого кута під плитку|Forming right angle for tiling|Herstellung eines rechten Winkels für Fliesenarbeiten|штукатурка,плитка
            putty|m2|Локальне шпаклювання після перенесення розеток|Local puttying after socket relocation|Lokales Spachteln nach Versetzen von Steckdosen|шпаклівка,електрика
            putty|m2|Фінішна доводка стін під декоративну фарбу|Finish preparation of walls for decorative paint|Feinvorbereitung von Wänden für Dekorfarbe|шпаклівка,декор
            painting|linear_m|Фарбування дверних укосів|Painting door reveals|Streichen von Türlaibungen|фарбування,двері
            painting|m2|Фарбування стін вологостійкою фарбою|Painting walls with moisture-resistant paint|Streichen von Wänden mit Feuchtraumfarbe|фарбування,санвузол
            wallpaper|m2|Поклейка шпалер у дитячій кімнаті|Hanging wallpaper in children's room|Tapezieren im Kinderzimmer|шпалери,кімната
            wallpaper|m2|Поклейка шпалер у коридорі|Hanging wallpaper in hallway|Tapezieren im Flur|шпалери,коридор
            tiling|m2|Укладання плитки у душовій зоні|Installing tiles in shower area|Verlegen von Fliesen im Duschbereich|плитка,душ
            tiling|m2|Укладання плитки у санвузлі на підлогу|Installing bathroom floor tiles|Verlegen von Bodenfliesen im Bad|плитка,санвузол
            tiling|m2|Укладання плитки у санвузлі на стіни|Installing bathroom wall tiles|Verlegen von Wandfliesen im Bad|плитка,санвузол
            tiling|linear_m|Формування зовнішнього кута плитки профілем|Forming external tile corner with trim|Ausbildung einer Außenecke mit Fliesenprofil|плитка,кут
            flooring|m2|Гідроізоляція підлоги перед стяжкою|Waterproofing floor before screed|Abdichtung des Bodens vor Estrich|підлога,гідроізоляція
            flooring|m2|Укладання кварц-вінілової плитки|Installing quartz vinyl tile|Verlegen von Quarz-Vinyl-Fliesen|підлога,кварц-вініл
            flooring|m2|Укладання інженерної дошки|Installing engineered flooring board|Verlegen von Mehrschichtdielen|підлога,інженерна дошка
            flooring|linear_m|Монтаж прихованого плінтуса|Installing concealed skirting board|Montage einer verdeckten Sockelleiste|підлога,плінтус
            drywall|m2|Монтаж гіпсокартонної перегородки з шумоізоляцією|Installing sound-insulated drywall partition|Montage einer schallgedämmten Gipskartontrennwand|гіпсокартон,шумоізоляція
            drywall|linear_m|Монтаж короба під інсталяцію унітаза|Installing drywall box for toilet frame|Montage eines Gipskartonkastens für WC-Vorwand|гіпсокартон,санвузол
            ceiling|linear_m|Монтаж ніші під приховану LED-підсвітку|Installing niche for concealed LED lighting|Montage einer Nische für verdeckte LED-Beleuchtung|стеля,led
            ceiling|m2|Монтаж акустичної стелі|Installing acoustic ceiling|Montage einer Akustikdecke|стеля,акустика
            electrical|point|Монтаж силової точки для посудомийної машини|Installing power point for dishwasher|Montage eines Stromanschlusses für Geschirrspüler|електрика,кухня
            electrical|point|Монтаж силової точки для пральної машини|Installing power point for washing machine|Montage eines Stromanschlusses für Waschmaschine|електрика,санвузол
            electrical|point|Монтаж силової точки для духової шафи|Installing power point for oven|Montage eines Stromanschlusses für Backofen|електрика,кухня
            electrical|point|Монтаж виводу під рушникосушарку електричну|Installing outlet for electric towel warmer|Montage eines Anschlusses für elektrischen Handtuchheizkörper|електрика,санвузол
            electrical|piece|Монтаж контактора в електрощиті|Installing contactor in distribution board|Montage eines Schützes im Verteiler|електрика,щит
            lighting|piece|Монтаж бра настінного|Installing wall sconce|Montage einer Wandleuchte|освітлення,бра
            lighting|piece|Монтаж підсвітки кухонного фартуха|Installing kitchen backsplash lighting|Montage einer Küchenrückwandbeleuchtung|освітлення,кухня
            low_voltage|point|Монтаж точки під відеодомофон|Installing video intercom point|Montage eines Video-Gegensprechanschlusses|слаботочка,домофон
            low_voltage|piece|Монтаж патч-панелі у слабкострумовому щиті|Installing patch panel in low-voltage cabinet|Montage eines Patchpanels im Schwachstromschrank|слаботочка,щит
            plumbing|point|Монтаж точки під кухонну мийку|Installing plumbing point for kitchen sink|Montage eines Sanitäranschlusses für Küchenspüle|сантехніка,кухня
            plumbing|point|Монтаж точки під посудомийну машину|Installing plumbing point for dishwasher|Montage eines Wasseranschlusses für Geschirrspüler|сантехніка,кухня
            plumbing|piece|Монтаж гігієнічного душу|Installing bidet shower|Montage einer Handbrause am WC|сантехніка,санвузол
            plumbing|piece|Монтаж рушникосушарки водяної|Installing hydronic towel warmer|Montage eines Warmwasser-Handtuchheizkörpers|сантехніка,рушникосушка
            sewerage|point|Монтаж каналізаційного виводу під пральну машину|Installing sewer outlet for washing machine|Montage eines Abwasseranschlusses für Waschmaschine|каналізація,техніка
            sewerage|point|Монтаж каналізаційного виводу під посудомийну машину|Installing sewer outlet for dishwasher|Montage eines Abwasseranschlusses für Geschirrspüler|каналізація,кухня
            heating|m2|Монтаж електричної теплої підлоги матами|Installing electric underfloor heating mats|Montage elektrischer Fußbodenheizmatten|опалення,тепла підлога
            heating|piece|Монтаж електричної рушникосушарки|Installing electric towel warmer|Montage eines elektrischen Handtuchheizkörpers|опалення,рушникосушка
            ventilation|piece|Монтаж витяжного вентилятора з таймером|Installing exhaust fan with timer|Montage eines Abluftventilators mit Timer|вентиляція,таймер
            ventilation|piece|Монтаж вентиляційної решітки у двері санвузла|Installing ventilation grille in bathroom door|Montage eines Lüftungsgitters in Badezimmertür|вентиляція,двері
            windows|piece|Заміна склопакета|Replacing insulated glass unit|Austausch einer Isolierglasscheibe|вікна,склопакет
            windows|linear_m|Монтаж пароізоляційної стрічки вікна|Installing window vapour barrier tape|Montage eines Fenster-Dampfsperrbands|вікна,стрічка
            doors|piece|Регулювання міжкімнатних дверей|Adjusting interior door|Einstellung einer Innentür|двері,регулювання
            doors|piece|Монтаж дверного обмежувача|Installing door stopper|Montage eines Türstoppers|двері,фурнітура
            cleaning|m2|Очищення сантехнічних приладів після монтажу|Cleaning sanitary fixtures after installation|Reinigung von Sanitärkeramik nach Montage|прибирання,сантехніка
            cleaning|m2|Очищення скляних душових перегородок|Cleaning glass shower partitions|Reinigung von Glas-Duschtrennwänden|прибирання,душ
            """;

        private const string MaterialData = """
            demolition|pack|Мішки поліпропіленові для будівельного сміття|Polypropylene bags for construction waste|Polypropylen-Säcke für Bauschutt|сміття,мішки
            demolition|roll|Плівка захисна будівельна|Protective construction film|Bau-Schutzfolie|захист,плівка
            demolition|piece|Контейнер для будівельного сміття|Construction waste container|Container für Bauschutt|сміття,контейнер
            demolition|piece|Диск алмазний для демонтажу|Diamond demolition disc|Diamanttrennscheibe für Abbruch|інструмент,диск
            demolition|piece|Зубило плоске для перфоратора|Flat chisel for rotary hammer|Flachmeißel für Bohrhammer|інструмент,зубило
            preparation|roll|Плівка поліетиленова захисна|Polyethylene protective film|Polyethylen-Schutzfolie|захист,плівка
            preparation|roll|Картон захисний для підлоги|Protective floor cardboard|Schutzkarton für Böden|захист,підлога
            preparation|roll|Стрічка малярна|Masking tape|Malerband|захист,стрічка
            preparation|liter|Антисептик для мінеральних поверхонь|Antiseptic for mineral surfaces|Antiseptikum für mineralische Oberflächen|антисептик,стіни
            preparation|bucket|Бетоноконтакт|Concrete contact primer|Betonkontakt|ґрунтовка,бетон
            preparation|liter|Знежирювач будівельний|Construction degreaser|Bau-Entfetter|очищення,знежирення
            preparation|piece|Маяк розмічальний лазерний|Laser marking target|Laser-Markierziel|розмітка,лазер
            foundation|m3|Пісок річковий митий|Washed river sand|Gewaschener Flusssand|фундамент,пісок
            foundation|m3|Щебінь гранітний фракція 5-20|Granite crushed stone fraction 5-20|Granit-Schotter Körnung 5-20|фундамент,щебінь
            foundation|m3|Бетон В20|Concrete B20|Beton B20|фундамент,бетон
            foundation|kg|Арматура А500С 10 мм|A500C rebar 10 mm|Bewehrungsstahl A500C 10 mm|фундамент,арматура
            foundation|kg|Арматура А500С 12 мм|A500C rebar 12 mm|Bewehrungsstahl A500C 12 mm|фундамент,арматура
            foundation|kg|Дріт в'язальний|Tie wire|Bindedraht|фундамент,дріт
            foundation|sheet|Фанера для опалубки|Formwork plywood|Schalungssperrholz|фундамент,опалубка
            foundation|m2|Геотекстиль дорожній|Road geotextile|Straßengeotextil|фундамент,геотекстиль
            foundation|bucket|Мастика бітумна гідроізоляційна|Bitumen waterproofing mastic|Bitumen-Dichtmasse|гідроізоляція,мастика
            foundation|slab|Екструдований пінополістирол XPS для цоколю|XPS board for plinth insulation|XPS-Platte für Sockeldämmung|утеплення,xps
            foundation|linear_m|Труба дренажна перфорована|Perforated drainage pipe|Perforiertes Drainagerohr|дренаж,труба
            masonry|piece|Газоблок 100 мм|Aerated concrete block 100 mm|Porenbetonstein 100 mm|кладка,газоблок
            masonry|piece|Газоблок 150 мм|Aerated concrete block 150 mm|Porenbetonstein 150 mm|кладка,газоблок
            masonry|piece|Керамічний блок перегородковий|Ceramic partition block|Keramischer Trennwandblock|кладка,керамоблок
            masonry|piece|Цегла повнотіла рядова|Solid common brick|Vollziegel Normalformat|кладка,цегла
            masonry|piece|Цегла пустотіла керамічна|Hollow ceramic brick|Hohlziegel keramisch|кладка,цегла
            masonry|piece|Цегла облицювальна|Facing brick|Verblendziegel|кладка,облицювання
            masonry|bag|Клей для газоблоку|Adhesive for aerated concrete blocks|Kleber für Porenbetonsteine|кладка,клей
            masonry|bag|Розчин кладочний цементний|Cement masonry mortar|Zement-Mauermörtel|кладка,розчин
            masonry|linear_m|Перемичка залізобетонна|Reinforced concrete lintel|Stahlbetonsturz|кладка,перемичка
            masonry|m2|Сітка кладочна|Masonry reinforcement mesh|Mauerwerksgitter|кладка,сітка
            plaster|bag|Штукатурка гіпсова машинна|Machine gypsum plaster|Maschinengipsputz|штукатурка,гіпс
            plaster|bag|Штукатурка гіпсова ручна|Manual gypsum plaster|Handgipsputz|штукатурка,гіпс
            plaster|bag|Штукатурка цементно-піщана|Cement-sand plaster|Zement-Sand-Putz|штукатурка,цемент
            plaster|bucket|Ґрунтовка глибокопроникна|Deep penetrating primer|Tiefengrund|ґрунтовка,стіни
            plaster|bucket|Бетоноконтакт для штукатурки|Concrete contact primer for plaster|Betonkontakt für Putz|ґрунтовка,бетон
            plaster|linear_m|Маяк штукатурний оцинкований 6 мм|Galvanized plaster beacon 6 mm|Verzinkte Putzschiene 6 mm|штукатурка,маяк
            plaster|linear_m|Кутник штукатурний перфорований|Perforated plaster corner bead|Perforiertes Putzeckprofil|штукатурка,кутник
            plaster|m2|Сітка штукатурна скловолоконна|Fiberglass plaster mesh|Glasfaser-Putzgewebe|штукатурка,сітка
            plaster|bag|Суміш ремонтна цементна|Cement repair compound|Zementärer Reparaturmörtel|штукатурка,ремонт
            plaster|liter|Пластифікатор для розчину|Mortar plasticizer|Mörtelplastifizierer|штукатурка,добавка
            putty|bag|Шпаклівка стартова гіпсова|Base gypsum putty|Gips-Grundspachtel|шпаклівка,старт
            putty|bag|Шпаклівка фінішна полімерна|Finish polymer putty|Polymer-Feinspachtel|шпаклівка,фініш
            putty|bag|Шпаклівка вологостійка|Moisture-resistant putty|Feuchtraumspachtel|шпаклівка,волога
            putty|roll|Стрічка серпянка|Self-adhesive joint tape|Fugenband selbstklebend|шпаклівка,стики
            putty|roll|Стрічка паперова армувальна|Paper reinforcing tape|Papier-Fugenband|шпаклівка,стики
            putty|roll|Склохолст малярний|Painter fiberglass fleece|Maler-Glasvlies|склохолст,стіни
            putty|linear_m|Кутник малярний алюмінієвий|Aluminium painter corner bead|Aluminium-Malereckprofil|шпаклівка,кутник
            putty|piece|Шліфувальна сітка P120|Sanding mesh P120|Schleifgitter P120|шпаклівка,шліфування
            putty|piece|Шліфувальна сітка P180|Sanding mesh P180|Schleifgitter P180|шпаклівка,шліфування
            painting|liter|Фарба інтер'єрна матова|Interior matt paint|Matte Innenfarbe|фарба,стіни
            painting|liter|Фарба інтер'єрна миюча|Washable interior paint|Waschbeständige Innenfarbe|фарба,стіни
            painting|liter|Фарба для стелі|Ceiling paint|Deckenfarbe|фарба,стеля
            painting|liter|Фарба фасадна акрилова|Acrylic facade paint|Acryl-Fassadenfarbe|фарба,фасад
            painting|liter|Емаль для радіаторів|Radiator enamel|Heizkörperlack|фарба,радіатори
            painting|liter|Емаль алкідна для металу|Alkyd enamel for metal|Alkydlack für Metall|фарба,метал
            painting|liter|Лак паркетний водний|Water-based parquet varnish|Wasserbasierter Parkettlack|лак,паркет
            painting|bucket|Декоративна штукатурка акрилова|Acrylic decorative plaster|Acryl-Dekorputz|декор,штукатурка
            painting|liter|Колер для фарби|Paint tint|Farbtonpaste|фарба,колер
            painting|roll|Валик малярний|Paint roller|Farbroller|інструмент,валик
            wallpaper|roll|Шпалери паперові|Paper wallpaper|Papiertapete|шпалери,папір
            wallpaper|roll|Шпалери флізелінові|Non-woven wallpaper|Vliestapete|шпалери,флізелін
            wallpaper|roll|Шпалери вінілові|Vinyl wallpaper|Vinyltapete|шпалери,вініл
            wallpaper|roll|Склошпалери|Glass fibre wallpaper|Glasfasertapete|шпалери,скло
            wallpaper|roll|Фотошпалери|Photo wallpaper|Fototapete|шпалери,фото
            wallpaper|pack|Клей для паперових шпалер|Adhesive for paper wallpaper|Kleber für Papiertapeten|шпалери,клей
            wallpaper|pack|Клей для флізелінових шпалер|Adhesive for non-woven wallpaper|Kleber für Vliestapeten|шпалери,клей
            wallpaper|pack|Клей для вінілових шпалер|Adhesive for vinyl wallpaper|Kleber für Vinyltapeten|шпалери,клей
            wallpaper|linear_m|Бордюр декоративний для шпалер|Decorative wallpaper border|Dekorative Tapetenbordüre|шпалери,бордюр
            tiling|m2|Плитка керамічна настінна|Ceramic wall tile|Keramische Wandfliese|плитка,стіни
            tiling|m2|Плитка керамічна підлогова|Ceramic floor tile|Keramische Bodenfliese|плитка,підлога
            tiling|m2|Керамограніт|Porcelain stoneware tile|Feinsteinzeugfliese|плитка,керамограніт
            tiling|m2|Керамограніт великоформатний|Large-format porcelain stoneware|Großformatiges Feinsteinzeug|плитка,керамограніт
            tiling|m2|Мозаїка скляна|Glass mosaic|Glasmosaik|плитка,мозаїка
            tiling|bag|Клей плитковий C1|Tile adhesive C1|Fliesenkleber C1|плитка,клей
            tiling|bag|Клей плитковий C2|Tile adhesive C2|Fliesenkleber C2|плитка,клей
            tiling|bag|Клей для великоформатного керамограніту|Adhesive for large-format porcelain tile|Kleber für großformatiges Feinsteinzeug|плитка,клей
            tiling|kg|Фуга цементна|Cement grout|Zementfuge|плитка,фуга
            tiling|kg|Фуга епоксидна|Epoxy grout|Epoxidfuge|плитка,фуга
            tiling|cartridge|Силікон санітарний|Sanitary silicone|Sanitärsilikon|плитка,силікон
            tiling|linear_m|Профіль плитковий алюмінієвий|Aluminium tile trim|Aluminium-Fliesenprofil|плитка,профіль
            tiling|pack|Хрестики для плитки|Tile spacers|Fliesenkreuze|плитка,хрестики
            tiling|pack|Система вирівнювання плитки|Tile levelling system|Fliesennivelliersystem|плитка,свп
            flooring|bag|Цемент М500|Cement M500|Zement M500|стяжка,цемент
            flooring|m3|Пісок для стяжки|Screed sand|Estrichsand|стяжка,пісок
            flooring|bag|Суміш для стяжки підлоги|Floor screed mix|Estrichmischung|стяжка,суміш
            flooring|bag|Самовирівнювальна суміш|Self-levelling compound|Selbstnivellierende Masse|наливна,суміш
            flooring|m2|Сітка армувальна для стяжки|Reinforcement mesh for screed|Bewehrungsgitter für Estrich|стяжка,сітка
            flooring|linear_m|Стрічка демпферна|Perimeter expansion tape|Randdämmstreifen|стяжка,демпфер
            flooring|m2|Підкладка під ламінат|Laminate underlay|Laminatunterlage|ламінат,підкладка
            flooring|m2|Ламінат 32 клас|Laminate class 32|Laminat Klasse 32|ламінат,підлога
            flooring|m2|Ламінат 33 клас|Laminate class 33|Laminat Klasse 33|ламінат,підлога
            flooring|m2|SPC-плитка|SPC flooring|SPC-Bodenbelag|spc,підлога
            flooring|m2|Паркетна дошка|Engineered wood flooring|Parkettdiele|паркет,підлога
            flooring|m2|Лінолеум побутовий|Residential linoleum|Wohnlinoleum|лінолеум,підлога
            flooring|m2|Ковролін|Carpet flooring|Teppichboden|ковролін,підлога
            flooring|linear_m|Плінтус пластиковий|PVC skirting board|Kunststoffsockelleiste|плінтус,пвх
            flooring|linear_m|Плінтус МДФ|MDF skirting board|MDF-Sockelleiste|плінтус,мдф
            drywall|sheet|Гіпсокартон стіновий 12,5 мм|Wall drywall board 12.5 mm|Wandgipskartonplatte 12,5 mm|гіпсокартон,лист
            drywall|sheet|Гіпсокартон вологостійкий 12,5 мм|Moisture-resistant drywall board 12.5 mm|Feuchtraum-Gipskartonplatte 12,5 mm|гіпсокартон,вологостійкий
            drywall|sheet|Гіпсокартон вогнестійкий 12,5 мм|Fire-resistant drywall board 12.5 mm|Feuerschutz-Gipskartonplatte 12,5 mm|гіпсокартон,вогнестійкий
            drywall|linear_m|Профіль CD 60|CD 60 profile|CD-60-Profil|гіпсокартон,профіль
            drywall|linear_m|Профіль UD 27|UD 27 profile|UD-27-Profil|гіпсокартон,профіль
            drywall|linear_m|Профіль CW 50|CW 50 profile|CW-50-Profil|гіпсокартон,профіль
            drywall|linear_m|Профіль UW 50|UW 50 profile|UW-50-Profil|гіпсокартон,профіль
            drywall|pack|Саморізи по металу для гіпсокартону|Drywall screws for metal studs|Gipskartonschrauben für Metallprofile|гіпсокартон,саморізи
            drywall|piece|Підвіс прямий|Direct suspension hanger|Direktabhänger|гіпсокартон,підвіс
            drywall|piece|З'єднувач однорівневий краб|Single-level cross connector|Kreuzverbinder einstufig|гіпсокартон,краб
            drywall|piece|Дюбель швидкого монтажу|Quick installation dowel|Schnellmontagedübel|гіпсокартон,дюбель
            drywall|roll|Стрічка ущільнювальна для профілю|Sealing tape for profile|Dichtungsband für Profile|гіпсокартон,стрічка
            drywall|m2|Мінеральна вата для перегородок|Mineral wool for partitions|Mineralwolle für Trennwände|гіпсокартон,ізоляція
            ceiling|m2|Полотно натяжної стелі ПВХ|PVC stretch ceiling membrane|PVC-Spanndeckenfolie|стеля,натяжна
            ceiling|m2|Полотно натяжної стелі тканинне|Fabric stretch ceiling membrane|Stoff-Spanndecke|стеля,натяжна
            ceiling|linear_m|Профіль для натяжної стелі|Stretch ceiling profile|Profil für Spanndecke|стеля,профіль
            ceiling|linear_m|Тіньовий профіль стелі|Shadow gap ceiling profile|Schattenfugenprofil Decke|стеля,профіль
            ceiling|linear_m|Багет стельовий поліуретановий|Polyurethane ceiling moulding|Polyurethan-Deckenleiste|стеля,багет
            ceiling|piece|Платформа під світильник|Light fixture mounting platform|Montageplattform für Leuchte|стеля,освітлення
            ceiling|piece|Термокільце для натяжної стелі|Thermal ring for stretch ceiling|Thermoring für Spanndecke|стеля,натяжна
            ceiling|m2|Плита касетної стелі|Suspended ceiling tile|Rasterdeckenplatte|стеля,касетна
            electrical|linear_m|Кабель ВВГнг 3х1,5|VVGng cable 3x1.5|VVGng-Kabel 3x1,5|електрика,кабель
            electrical|linear_m|Кабель ВВГнг 3х2,5|VVGng cable 3x2.5|VVGng-Kabel 3x2,5|електрика,кабель
            electrical|linear_m|Кабель ВВГнг 3х4|VVGng cable 3x4|VVGng-Kabel 3x4|електрика,кабель
            electrical|linear_m|Гофротруба ПВХ 16 мм|PVC corrugated conduit 16 mm|PVC-Wellrohr 16 mm|електрика,гофра
            electrical|linear_m|Гофротруба ПВХ 20 мм|PVC corrugated conduit 20 mm|PVC-Wellrohr 20 mm|електрика,гофра
            electrical|piece|Підрозетник для бетону|Socket box for concrete|Unterputzdose für Beton|електрика,підрозетник
            electrical|piece|Підрозетник для гіпсокартону|Socket box for drywall|Hohlwanddose für Gipskarton|електрика,підрозетник
            electrical|piece|Розетка одинарна|Single power socket|Einzelsteckdose|електрика,розетка
            electrical|piece|Розетка подвійна|Double power socket|Doppelsteckdose|електрика,розетка
            electrical|piece|Вимикач одноклавішний|Single-gang switch|Einfachschalter|електрика,вимикач
            electrical|piece|Вимикач двоклавішний|Two-gang switch|Doppelschalter|електрика,вимикач
            electrical|piece|Автоматичний вимикач 16А|Circuit breaker 16A|Leitungsschutzschalter 16A|електрика,автомат
            electrical|piece|Автоматичний вимикач 25А|Circuit breaker 25A|Leitungsschutzschalter 25A|електрика,автомат
            electrical|piece|Диференційний автомат 16А|RCBO 16A|FI/LS-Schalter 16A|електрика,захист
            electrical|piece|Реле контролю напруги|Voltage monitoring relay|Spannungsüberwachungsrelais|електрика,захист
            electrical|piece|Електрощит квартирний|Apartment distribution board|Wohnungsverteiler|електрика,щит
            lighting|piece|Світильник точковий врізний|Recessed spotlight|Einbaustrahler|освітлення,світильник
            lighting|piece|Світильник накладний|Surface-mounted light|Aufbauleuchte|освітлення,світильник
            lighting|piece|Люстра стельова|Ceiling chandelier|Deckenkronleuchter|освітлення,люстра
            lighting|linear_m|LED-стрічка 12В|LED strip 12V|LED-Streifen 12V|освітлення,led
            lighting|linear_m|LED-стрічка 24В|LED strip 24V|LED-Streifen 24V|освітлення,led
            lighting|linear_m|Профіль алюмінієвий для LED|Aluminium profile for LED|Aluminiumprofil für LED|освітлення,профіль
            lighting|piece|Блок живлення LED|LED power supply|LED-Netzteil|освітлення,живлення
            lighting|piece|Димер для LED|LED dimmer|LED-Dimmer|освітлення,димер
            lighting|piece|Датчик руху|Motion sensor|Bewegungsmelder|освітлення,датчик
            lighting|piece|Трекова шина|Track rail|Stromschiene|освітлення,трек
            lighting|piece|Трековий світильник|Track light fixture|Schienenleuchte|освітлення,трек
            low_voltage|linear_m|Кабель UTP Cat.5e|UTP cable Cat.5e|UTP-Kabel Cat.5e|слаботочка,інтернет
            low_voltage|linear_m|Кабель UTP Cat.6|UTP cable Cat.6|UTP-Kabel Cat.6|слаботочка,інтернет
            low_voltage|linear_m|Кабель коаксіальний RG-6|Coaxial cable RG-6|Koaxialkabel RG-6|слаботочка,тв
            low_voltage|piece|Інтернет-розетка RJ45|RJ45 data outlet|RJ45-Netzwerkdose|слаботочка,інтернет
            low_voltage|piece|Телевізійна розетка|TV outlet|TV-Dose|слаботочка,тв
            low_voltage|piece|IP-камера внутрішня|Indoor IP camera|IP-Kamera innen|слаботочка,камера
            low_voltage|piece|Wi-Fi точка доступу|Wi-Fi access point|WLAN-Access-Point|слаботочка,wifi
            low_voltage|piece|Шафа слабкострумова|Low-voltage cabinet|Schwachstromschrank|слаботочка,шафа
            plumbing|linear_m|Труба PPR 20 мм|PPR pipe 20 mm|PPR-Rohr 20 mm|сантехніка,труба
            plumbing|linear_m|Труба PPR 25 мм|PPR pipe 25 mm|PPR-Rohr 25 mm|сантехніка,труба
            plumbing|linear_m|Труба PEX 16 мм|PEX pipe 16 mm|PEX-Rohr 16 mm|сантехніка,труба
            plumbing|linear_m|Труба PEX 20 мм|PEX pipe 20 mm|PEX-Rohr 20 mm|сантехніка,труба
            plumbing|piece|Колектор водопостачання|Water supply manifold|Wasserverteiler|сантехніка,колектор
            plumbing|piece|Кран кульовий|Ball valve|Kugelhahn|сантехніка,кран
            plumbing|piece|Фільтр грубого очищення води|Coarse water filter|Grobwasserfilter|сантехніка,фільтр
            plumbing|piece|Редуктор тиску води|Water pressure reducing valve|Wasserdruckminderer|сантехніка,редуктор
            plumbing|piece|Лічильник холодної води|Cold water meter|Kaltwasserzähler|сантехніка,лічильник
            plumbing|piece|Лічильник гарячої води|Hot water meter|Warmwasserzähler|сантехніка,лічильник
            plumbing|piece|Інсталяція для унітаза|Concealed toilet frame|WC-Vorwandelement|сантехніка,унітаз
            plumbing|piece|Унітаз підвісний|Wall-hung toilet|Wandhängendes WC|сантехніка,унітаз
            plumbing|piece|Умивальник керамічний|Ceramic washbasin|Keramikwaschbecken|сантехніка,умивальник
            plumbing|piece|Змішувач для умивальника|Washbasin mixer tap|Waschtischarmatur|сантехніка,змішувач
            plumbing|piece|Ванна акрилова|Acrylic bathtub|Acrylbadewanne|сантехніка,ванна
            plumbing|piece|Душова система|Shower system|Duschsystem|сантехніка,душ
            sewerage|linear_m|Труба каналізаційна ПВХ 50 мм|PVC sewer pipe 50 mm|PVC-Abwasserrohr 50 mm|каналізація,труба
            sewerage|linear_m|Труба каналізаційна ПВХ 110 мм|PVC sewer pipe 110 mm|PVC-Abwasserrohr 110 mm|каналізація,труба
            sewerage|piece|Коліно каналізаційне 50 мм|Sewer elbow 50 mm|Abwasserbogen 50 mm|каналізація,фітинг
            sewerage|piece|Коліно каналізаційне 110 мм|Sewer elbow 110 mm|Abwasserbogen 110 mm|каналізація,фітинг
            sewerage|piece|Трійник каналізаційний 50 мм|Sewer tee 50 mm|Abwasser-T-Stück 50 mm|каналізація,фітинг
            sewerage|piece|Трійник каналізаційний 110 мм|Sewer tee 110 mm|Abwasser-T-Stück 110 mm|каналізація,фітинг
            sewerage|piece|Ревізія каналізаційна|Sewer inspection fitting|Abwasserrevision|каналізація,ревізія
            sewerage|piece|Трап душовий|Shower floor drain|Duschablauf|каналізація,трап
            sewerage|piece|Сифон для умивальника|Washbasin siphon|Waschbeckensiphon|каналізація,сифон
            sewerage|piece|Сифон для ванни|Bathtub siphon|Badewannensiphon|каналізація,сифон
            heating|piece|Радіатор сталевий панельний|Steel panel radiator|Stahlplattenheizkörper|опалення,радіатор
            heating|section|Радіатор біметалевий секційний|Sectional bimetal radiator|Bimetall-Gliederheizkörper|опалення,радіатор
            heating|piece|Клапан термостатичний|Thermostatic valve|Thermostatventil|опалення,клапан
            heating|piece|Термоголовка радіаторна|Radiator thermostatic head|Heizkörper-Thermostatkopf|опалення,термоголовка
            heating|piece|Кран Маєвського|Mayevsky air vent|Entlüftungsventil|опалення,кран
            heating|linear_m|Труба PEX для опалення 16 мм|PEX heating pipe 16 mm|PEX-Heizungsrohr 16 mm|опалення,труба
            heating|linear_m|Труба PEX для опалення 20 мм|PEX heating pipe 20 mm|PEX-Heizungsrohr 20 mm|опалення,труба
            heating|m2|Мат теплої підлоги з фіксаторами|Underfloor heating mat with clips|Fußbodenheizungsmatte mit Haltern|опалення,тепла підлога
            heating|piece|Колектор теплої підлоги|Underfloor heating manifold|Fußbodenheizungsverteiler|опалення,колектор
            heating|piece|Насосно-змішувальний вузол|Pump mixing unit|Pumpenmischgruppe|опалення,вузол
            ventilation|piece|Вентилятор витяжний побутовий|Domestic exhaust fan|Haushalts-Abluftventilator|вентиляція,вентилятор
            ventilation|piece|Клапан зворотний вентиляційний|Ventilation backdraft damper|Rückstauklappe Lüftung|вентиляція,клапан
            ventilation|linear_m|Повітровід пластиковий круглий|Round plastic air duct|Runder Kunststoff-Luftkanal|вентиляція,повітровід
            ventilation|linear_m|Повітровід пластиковий плоский|Flat plastic air duct|Flacher Kunststoff-Luftkanal|вентиляція,повітровід
            ventilation|linear_m|Повітровід гнучкий алюмінієвий|Flexible aluminium air duct|Flexibler Aluminium-Luftkanal|вентиляція,повітровід
            ventilation|piece|Решітка вентиляційна|Ventilation grille|Lüftungsgitter|вентиляція,решітка
            ventilation|piece|Клапан припливний стіновий|Wall air inlet valve|Wand-Zuluftventil|вентиляція,приплив
            ventilation|piece|Хомут для повітроводу|Air duct clamp|Luftkanalschelle|вентиляція,кріплення
            windows|piece|Вікно металопластикове|PVC window|Kunststofffenster|вікна,пвх
            windows|piece|Вікно алюмінієве|Aluminium window|Aluminiumfenster|вікна,алюміній
            windows|linear_m|Підвіконня ПВХ|PVC window sill|PVC-Fensterbank|вікна,підвіконня
            windows|linear_m|Підвіконня дерев'яне|Wooden window sill|Holzfensterbank|вікна,підвіконня
            windows|linear_m|Відлив зовнішній металевий|Metal exterior window sill|Metall-Außenfensterbank|вікна,відлив
            windows|linear_m|Сендвіч-панель для укосів|Sandwich panel for reveals|Sandwichpaneel für Laibungen|вікна,укоси
            windows|cartridge|Герметик віконний|Window sealant|Fensterdichtstoff|вікна,герметик
            windows|piece|Москітна сітка|Mosquito screen|Insektenschutzgitter|вікна,сітка
            doors|piece|Двері міжкімнатні полотном|Interior door leaf|Innentürblatt|двері,полотно
            doors|piece|Дверний короб міжкімнатний|Interior door frame|Innentürzarge|двері,короб
            doors|set|Дверний блок міжкімнатний|Interior door set|Innentürblock|двері,комплект
            doors|piece|Двері прихованого монтажу|Hidden door|Verdeckte Tür|двері,приховані
            doors|piece|Двері вхідні металеві|Metal entrance door|Metall-Eingangstür|двері,вхідні
            doors|set|Система розсувних дверей|Sliding door system|Schiebetürsystem|двері,розсувні
            doors|linear_m|Лиштва дверна|Door casing|Türbekleidung|двері,лиштва
            doors|piece|Петля дверна|Door hinge|Türband|двері,петля
            doors|piece|Замок дверний|Door lock|Türschloss|двері,замок
            doors|piece|Ручка дверна|Door handle|Türgriff|двері,ручка
            doors|cartridge|Піна монтажна дверна|Door installation foam|Montageschaum für Türen|двері,піна
            facade|bucket|Ґрунтовка фасадна|Facade primer|Fassadengrundierung|фасад,ґрунт
            facade|bag|Штукатурка фасадна цементна|Cement facade plaster|Zement-Fassadenputz|фасад,штукатурка
            facade|slab|Пінополістирол фасадний EPS|Facade EPS board|Fassaden-EPS-Platte|фасад,утеплення
            facade|slab|Мінеральна вата фасадна|Facade mineral wool board|Fassaden-Mineralwollplatte|фасад,утеплення
            facade|m2|Склосітка фасадна|Facade fiberglass mesh|Fassaden-Glasfasergewebe|фасад,сітка
            facade|linear_m|Кутник фасадний з сіткою|Facade corner bead with mesh|Fassadeneckprofil mit Gewebe|фасад,кутник
            facade|linear_m|Профіль цокольний|Plinth profile|Sockelprofil|фасад,профіль
            facade|bucket|Штукатурка декоративна фасадна|Decorative facade plaster|Dekorativer Fassadenputz|фасад,декор
            facade|liter|Фарба фасадна силіконова|Silicone facade paint|Silikon-Fassadenfarbe|фасад,фарба
            roofing|m2|Металочерепиця|Metal roof tile|Metallziegel|покрівля,металочерепиця
            roofing|m2|Бітумна черепиця|Bitumen shingles|Bitumenschindeln|покрівля,черепиця
            roofing|m2|Профнастил покрівельний|Corrugated roofing sheet|Trapezblech für Dach|покрівля,профнастил
            roofing|linear_m|Коньок покрівельний|Roof ridge cap|Dachfirstelement|покрівля,коньок
            roofing|linear_m|Планка карнизна|Eaves flashing|Traufleiste|покрівля,планка
            roofing|linear_m|Жолоб водостічний|Gutter|Dachrinne|покрівля,водостік
            roofing|linear_m|Труба водостічна|Downpipe|Fallrohr|покрівля,водостік
            roofing|m2|Мембрана гідроізоляційна покрівельна|Roof waterproofing membrane|Dachabdichtungsbahn|покрівля,мембрана
            roofing|m2|Мембрана пароізоляційна покрівельна|Roof vapour barrier membrane|Dachdampfsperrbahn|покрівля,пароізоляція
            roofing|linear_m|Дошка обрізна для обрешітки|Edged board for roof battens|Brett für Dachlattung|покрівля,обрешітка
            insulation|slab|Мінеральна вата універсальна|Universal mineral wool board|Universelle Mineralwollplatte|ізоляція,вата
            insulation|slab|Мінеральна вата акустична|Acoustic mineral wool board|Akustik-Mineralwollplatte|ізоляція,акустика
            insulation|slab|Екструдований пінополістирол XPS|Extruded polystyrene XPS|Extrudierter Polystyrol XPS|ізоляція,xps
            insulation|m2|Мембрана пароізоляційна|Vapour barrier membrane|Dampfsperrbahn|ізоляція,пароізоляція
            insulation|m2|Мембрана гідроізоляційна|Waterproofing membrane|Abdichtungsbahn|ізоляція,гідроізоляція
            insulation|linear_m|Ізоляція трубна каучукова|Rubber pipe insulation|Kautschuk-Rohrisolierung|ізоляція,труби
            insulation|linear_m|Ізоляція трубна поліетиленова|Polyethylene pipe insulation|PE-Rohrisolierung|ізоляція,труби
            insulation|roll|Стрічка алюмінієва для ізоляції|Aluminium insulation tape|Aluminiumband für Dämmung|ізоляція,стрічка
            cleaning|liter|Засіб для видалення цементного нальоту|Cement residue remover|Zementschleierentferner|прибирання,плитка
            cleaning|liter|Засіб для миття підлоги після ремонту|Post-renovation floor cleaner|Bodenreiniger nach Renovierung|прибирання,підлога
            cleaning|liter|Засіб для очищення скла|Glass cleaner|Glasreiniger|прибирання,скло
            cleaning|roll|Серветки мікрофіброві|Microfiber cloths|Mikrofasertücher|прибирання,серветки
            cleaning|piece|Скребок для скла|Glass scraper|Glasschaber|прибирання,скло
            cleaning|pack|Мішки для будівельного сміття посилені|Heavy-duty construction waste bags|Starke Säcke für Bauschutt|прибирання,сміття
            outdoor|m2|Плитка тротуарна|Paving slab|Pflasterstein|благоустрій,плитка
            outdoor|linear_m|Бордюр тротуарний|Kerbstone|Bordstein|благоустрій,бордюр
            outdoor|m3|Щебінь для благоустрою|Crushed stone for landscaping|Schotter für Außenanlagen|благоустрій,щебінь
            outdoor|m3|Пісок для благоустрою|Sand for landscaping|Sand für Außenanlagen|благоустрій,пісок
            outdoor|linear_m|Лоток водовідвідний бетонний|Concrete drainage channel|Beton-Entwässerungsrinne|благоустрій,дренаж
            outdoor|linear_m|Труба дренажна геотекстильна|Geotextile-wrapped drainage pipe|Drainagerohr mit Geotextil|благоустрій,дренаж
            outdoor|m2|Дошка терасна композитна|Composite decking board|WPC-Terrassendiele|благоустрій,тераса
            outdoor|m2|Металопрофіль для паркану|Metal profile sheet for fence|Profilblech für Zaun|благоустрій,паркан
            """;
    }
}
