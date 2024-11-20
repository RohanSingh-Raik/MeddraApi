using MeddraService;

string filePath = @"C:\Users\RohanSingh\Desktop\MedDRA_27_1_English\MedDRA_27_1_English\MedAscii\mdhier.asc";

var meddraService = new MeddraApi(filePath);
var data = meddraService._meddraRecords;

var result = meddraService.GetHierarchyByTerm("Anaemia folate deficiency", "PT");

Console.ReadKey();
