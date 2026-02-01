using CLImate.App.Composition;
using CLImate.App.Cli;
using Microsoft.Extensions.DependencyInjection;

var provider = AppComposition.BuildServiceProvider();
var app = provider.GetRequiredService<ICliApplication>();
return await app.RunAsync(args);
