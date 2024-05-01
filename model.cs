using System.ComponentModel.DataAnnotations;

namespace TMS_API;

public class TMS_APP
{
    private string _email = string.Empty;
    private string _pwd = string.Empty;
    
    [Required]
    public string email
    {
        get {return _email;}
        set {if (value == null) throw new ArgumentNullException(nameof(value)); _email = value;}
    }

    [Required]
    public string pwd 
    {
        get {return _pwd;}
        set  {if (value == null) throw new ArgumentNullException(nameof(value)); _pwd = value;}
    }

}
