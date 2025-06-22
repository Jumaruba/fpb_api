using System.Text.RegularExpressions;

namespace Crawler.PdfParser;

public class LineDiscard
{
    private HashSet<string> _linesToDiscard =
    [
        "Dia Evento Localidade",
        "Abertura",
        "inscrições",
        "Fecho",
        "Desistência s/multa até",
        "às 12horas"
    ];
    
    public static Dictionary<string, string> MonthNameCompleteToNum = new()
    {
        {"Janeiro", "01"},
        {"Fevereiro", "02"},
        {"Março", "03"},
        {"Abril", "04"},
        {"Maio", "05"},
        {"Junho", "06"},
        {"Julho", "07"},
        {"Agosto", "08"},
        {"Setembro", "09"},
        {"Outubro", "10"},
        {"Novembro", "11"},
        {"Dezembro", "12"},
        {"None", "13"},
    };

    public LineDiscard()
    {
        var monthsToDiscard = GetMonthLinesToDiscard();
        _linesToDiscard.UnionWith(monthsToDiscard);
    }
    

    public bool ShouldDiscard(string line)
    {
        line = line.Trim();
        if (_linesToDiscard.Contains(line))
            return true;

        // At the end of each page, there is indication of currentPage/totalPages;
        // We want to discard this.
        return Regex.IsMatch(line, @"^\d/\d$");
    }
    
    /// <summary>
    /// We want to discard the lines indicating the month, since they are out-of-order.
    /// </summary>
    /// <returns>The possible lines indicating the months.</returns>
    private HashSet<string> GetMonthLinesToDiscard()
    {
        var currentYear = DateTime.Now.Year;                                                     
        var monthNames = MonthNameCompleteToNum.Keys;                                      
        return monthNames.Select(monthName => $"{monthName} {currentYear}").ToHashSet();   
    }

}