using System.ComponentModel.DataAnnotations.Schema;

namespace Models;

[Dapper.Contrib.Extensions.Table("mod_actions")]
public class ModActions
{
    [Column("id")]
    public string Id { get; set; }
    
    [Column("user_id")]
    public long UserId { get; set; }
    
    [Column("mod_id")]
    public long ModId { get; set; }
    
    [Column("type")]
    public string Type { get; set; }
    
    [Column("reason")]
    public string Reason { get; set; }
    
    [Column("date")]
    public DateTime Date { get; set; }
    
    [Column("until")]
    public DateTime Until { get; set; }
    
    [Column("server_id")]
    public long ServerId { get; set; }
    
    [Column("action_id")]
    public string ActionId { get; set; }
}