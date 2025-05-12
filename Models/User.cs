using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace rims.Models;

[Keyless]
[Table("users")]
public partial class User
{
    [Column("id")]
    public long Id { get; set; }

    [Column("email")]
    [StringLength(255)]
    public string Email { get; set; } = null!;

    [Column("user_id")]
    [StringLength(50)]
    public string UserId { get; set; } = null!;

    [Column("username")]
    [StringLength(255)]
    public string Username { get; set; } = null!;

    [Column("first_name")]
    [StringLength(255)]
    public string FirstName { get; set; } = null!;

    [Column("second_name")]
    [StringLength(255)]
    public string? SecondName { get; set; }

    [Column("last_name")]
    [StringLength(255)]
    public string LastName { get; set; } = null!;

    [Column("phone")]
    [StringLength(255)]
    public string? Phone { get; set; }

    [Column("country")]
    [StringLength(255)]
    public string? Country { get; set; }

    [Column("gender")]
    [StringLength(255)]
    public string? Gender { get; set; }

    [Column("position")]
    [StringLength(255)]
    public string? Position { get; set; }
}
