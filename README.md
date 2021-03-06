## DSL, T4, REST API, legacy database - code generation made easy
This project allows you to specify, using a mini DSL, REST API resources and their associated database queries, along 
with expansions that are supported. It is also possible to generate a basic REST API using one of the T4 templates.

This is particularly useful when:

* You have a back end database that does not use foreign keys to represent relationships
* Using entity framework independent associations has not worked or will not work
* There are more than a few resources each with a number of expansions

Using the configurable T4 templates, you can have the DSL (which has simple semantics) converted into:

* concrete (but partial) c# classes that expose a number of useful entry points, making the creation of a repository (for example) implementation simple, and allowing easy connectivity to API paging and expansion options
* a basic REST API, implementing resource collections, resources and sub resource access

## Executing the example
The VS 2015 solution uses .NET Core and .NET standard. The easiest way to see what this solution does (documentation follows this section) is to:

* Clone the repo/Download a zip
* Open the DSLT4RestApi solution
* Restore packages as necessary
* Build the solution
* Open the file DSLT4ConsoleApp/Program.cs - set a breakpoint at first call to ExecuteBlogExpandedQuery OR
* Run the DSLT4RestApi project, and navigate around
* F5, then F11 through to see the activity
* If you want to execute the T4 templates, right click on them and 'Run custom tool'

## Customisation
The T4 templates have some limited configuration capability, as below:

``` 
// Namespaces to include (for EF model and so on)
var includeNamespaces = new List<string> { "EF.Model" };
// The type of the EF context
var contextType = "BloggingContext"; 
// Base namespace for all generated objects
var baseNamespace = "Complex.Omnibus.Autogenerated.ExpansionHandling";
// The DSL instance file extension of interest (txt or json)
var srcFormat = "json";
// True i the advanced form of a DSL instance template should be used
var useAdvancedFormDSL = true;
// Form the dsl instance file name to use
var dslFile = "dsl-instance" + (useAdvancedFormDSL ? "-advanced" : string.Empty) + ".";
// Default top if none supplied
var defaultTop = 10;
// Default skip if none supplied
var defaultSkip = 0;
// True if the expansions passed in shold be checked
var checkExpansions = true;
// If true, then expansions should be title cased e.g. posts should be Posts, readers should be Readers and so on
var expansionsAreTitleCased = true;
```

All of these should be self explanatory.

# REST API - example use
Examine the output of the DSLRestApiGenerator.tt T4 template, and run the DSLT4RestApi project to see what is implemented. The start page should be 
a Swashbuckle generated UI for exploration. Start at perhaps /api/Blogs.

## The DSL
The 'grammar' of the DSL itself is straightforward, and can be represented in either a text or JSON object form. This section 
describes both.

* Each input DSL file can contain 1 or more resource descriptions
* Queries can be specified on a per resource or 'global' basis
* The markup {joins} and {extraWhere} are placeholders in a query that the DSL processor replaces with text it infers when processing a DSL instance

### Text form
A few points:
* Text file parsing is simplistic and failure to follow the basic rules will not be adaptively handled :-)
* Regardless of usage, a 'tag' keyword always introduces a new resource
* Blank lines are ignored
* Comments can be specified using a hash (#) at the start of the line 

Global
Using a defaultQuery (specified once) is useful if more than 1 resource exists and all use the same basic query form.

| Keyword        |Mandatory| Multi-line | After = | Semantics | If omitted |
|----------------|---|------------|---------|-----------|------------|
| defaultQuery |N|Y|N/A|Pro forma EF LINQ query with placeholders|N/A|

Suported placeholders:
* [set] - replaced by either the setName of a resource or the resource tag
* [baseEntity] - uses the singular name to form the first anonymous object creation in a query
* [orderProperty] - replaced by the orderProperty of the resource to support paging

Resource specification:

| Keyword        |Mandatory| Multi-line | After = | Semantics | If omitted |
|----------------|---|------------|---------|-----------|------------|
| tag |Y|N|string|Names a resource|N/A|
| singular-tag |N|N|string|Singular name of resource|tag value minus last character|
| model |N|N|string|EF entity name|singular-tag value|
| orderProperty |N|N|string|Entity property name for ordering|N/A|
| restResourceIdProperty |N|N|string|Property name that is the ID for REST purposes|N/A|
| restResourceIdPropertyType |N|N|string|Type name of the Rest resource id property|N/A|
| setName |N|N|string|Entity set name|tag value|
| baseQuery |N|Y|N/A|Pro forma EF LINQ query|Uses global defaultQuery|

Notes:
* If baseQuery is omitted, defaultQuery must have been specified
* If baseQuery is omitted, orderProperty must be given 

This section can then be followed by 1 or more expansion definitions, having the form:

| Keyword        |Mandatory| Multi-line | After = | Semantics | If omitted |
|----------------|---|------------|---------|-----------|------------|
| expansion |N|N|string|Names an expansion|N/A|
| N/A |Y|N|string|a c# type representing the basis of the expansion|N/A|
| N/A |Y|N|string|a join specification for the expansion|N/A|
| N/A |Y|N|string|a predicate for the join (can be empty)|N/A|

#### Example - using per resource queries
An example fully specified entry using the 'baseQuery' configuration:

```tag=Blogs
singular-tag=Blog
model=Blog
# API usage
restResourceIdProperty=BlogId
restResourceIdPropertyType=int
#
baseQuery=
(await ctx.Blogs
.AsNoTracking()
.Where(expr)
.Select(b => new { Blog = b })
{joins}
{extraWhere}
.OrderBy(a => a.Blog.BlogId)
.Skip(skip) 
.Take(top)
.ToListAsync())
#
expansion=Posts
IEnumerable<Post>
.GroupJoin(ctx.Posts, a => a.Blog.NonKeyField, post => post.NonKeyField, {selector})

#
expansion=Readers
IEnumerable<Party>
.GroupJoin(ctx.Parties.Where(r => r.Disposition == "reader"), a => a.Blog.NonKeyField, party => party.NonKeyField, {selector})

#
expansion=Author
Party
.Join(ctx.Parties, a => a.Blog.NonKeyField, party => party.NonKeyField, {selector})
.Where(p => p.Author.Disposition == "author")
```

#### Example - using default query
This is a possible 'global' default query:

```defaultQuery=
(await ctx.[set]
.AsNoTracking()
.Where(expr)
.Select(obj => new { [baseEntity] })
{joins}
{extraWhere}
.OrderBy(a => [orderProperty])
.Skip(skip) 
.Take(top)
.ToListAsync())
```
And a possible fully specified resource definition, noting that the specification of both model and singular-tag is not necessary:

```tag=Blogs
singular-tag=Blog
model=Blog
# API usage
restResourceIdProperty=BlogId
restResourceIdPropertyType=int
#
orderProperty=BlogId
#
expansion=Posts
IEnumerable<Post>
.GroupJoin(ctx.Posts, a => a.Blog.NonKeyField, post => post.NonKeyField, {selector})

#
expansion=Readers
IEnumerable<Party>
.GroupJoin(ctx.Parties.Where(r => r.Disposition == "reader"), a => a.Blog.NonKeyField, party => party.NonKeyField, {selector})

#
expansion=Author
Party
.Join(ctx.Parties, a => a.Blog.NonKeyField, party => party.NonKeyField, {selector})
.Where(p => p.Author.Disposition == "author")
```
#### Example DSL files
There are four files provided, all of which generate the same c# code, but are in two different formats (JSON and text) and use or do not use the global query capability.

