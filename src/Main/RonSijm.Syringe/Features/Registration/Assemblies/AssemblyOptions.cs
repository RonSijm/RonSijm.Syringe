﻿namespace RonSijm.Syringe;

public class AssemblyOptions
{
    /// <summary>
    /// Lets you define a custom class path
    /// </summary>
    public string ClassPath { get; set; }

    /// <summary>
    /// Disables CascadeLoading for a specific assembly
    /// </summary>
    public bool DisableCascadeLoading { get; set; }

    /// <summary>
    /// Configures a relative path to load a specific dll from
    /// </summary>
    public string RelativePath { get; set; }


    /// <summary>
    /// Configures an absolute path to load a specific dll from
    /// </summary>
    public string AbsolutePath { get; set; }

    /// <summary>
    /// Configures a Decoration on the HttpRequestMessage before it's send
    /// </summary>
    public Func<string, HttpRequestMessage, AssemblyOptions, bool> HttpHandler { get; set; }
}