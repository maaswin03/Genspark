using NpgsqlTypes;

namespace backend.Models.Enums;

public enum BusType
{
    [PgName("AC_Sleeper")]
    AC_Sleeper,

    [PgName("Non_AC_Sleeper")]
    Non_AC_Sleeper,

    [PgName("AC_Seater")]
    AC_Seater,

    [PgName("Non_AC_Seater")]
    Non_AC_Seater,

    [PgName("AC_Semi_Sleeper")]
    AC_Semi_Sleeper,

    [PgName("Non_AC_Semi_Sleeper")]
    Non_AC_Semi_Sleeper
}
