namespace GenMail.Core.Generation;

public static class BuiltInUsernameRules
{
    public static IReadOnlyList<IUsernameRule> Create() => new List<IUsernameRule>
    {
        new TemplateUsernameRule("first", "{first}"),
        new TemplateUsernameRule("last", "{last}"),
        new TemplateUsernameRule("firstlast", "{first}{last}"),
        new TemplateUsernameRule("lastfirst", "{last}{first}"),
        new TemplateUsernameRule("first.last", "{first}.{last}"),
        new TemplateUsernameRule("last.first", "{last}.{first}"),
        new TemplateUsernameRule("first_last", "{first}_{last}"),
        new TemplateUsernameRule("last_first", "{last}_{first}"),
        new TemplateUsernameRule("first-last", "{first}-{last}"),
        new TemplateUsernameRule("last-first", "{last}-{first}"),
        new TemplateUsernameRule("flast", "{fi}{last}"),
        new TemplateUsernameRule("firstl", "{first}{li}"),
        new TemplateUsernameRule("f.last", "{fi}.{last}"),
        new TemplateUsernameRule("first.l", "{first}.{li}"),
        new TemplateUsernameRule("firstmiddlelast", "{first}{middle}{last}"),
        new TemplateUsernameRule("first.middle.last", "{first}.{middle}.{last}"),
        new TemplateUsernameRule("all", "{all}"),
        new TemplateUsernameRule("all.dot", "{first}.{middle}.{last}"),
        new TemplateUsernameRule("reverse.all", "{reverseAll}"),
        new TemplateUsernameRule("first3last", "{first3}{last}"),
        new TemplateUsernameRule("firstlast3", "{first}{last3}"),
        new TemplateUsernameRule("first3last3", "{first3}{last3}")
    };
}
