using MeddraService;

string filePath = @"C:\Users\RohanSingh\Desktop\MedDRA_27_1_English\MedDRA_27_1_English\MedAscii\mdhier.asc";
string lltFilePath = @"C:\Users\RohanSingh\Desktop\MedDRA_27_1_English\MedDRA_27_1_English\MedAscii\llt.asc";

var meddraService = new MeddraApi(filePath,lltFilePath);
var data = meddraService._meddraRecords;
var data1 = meddraService._lltRecords;

var result = meddraService.GetHierarchyByTerm("Anaemia folate deficiency", "PT");
var result1 = meddraService.GetHierarchyByTerm("Haematological and lymphoid tissue therapeutic procedures", "HLGT");

var lltHeartResults = meddraService.SearchTerm("heart", "LLT");

var ptPainResults = meddraService.SearchTerm("pain", "PT");

var socResults = meddraService.SearchTerm("h","SOC");

var hlgtResults = meddraService.SearchTerm("w","HLGT");

var hltResults = meddraService.SearchTerm("l","HLT");

Console.ReadKey();
