namespace AI_genda_API.Abstractions;

public static class RegexPattern
{
    public static string Password =
       "^(?=.*[0-9])(?=.*[a-z])(?=.*[A-Z])(?=.*[^a-zA-Z0-9_]).{8,}$";

}
