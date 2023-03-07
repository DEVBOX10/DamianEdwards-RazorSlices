﻿using System.IO.Pipelines;
using System.Text.Encodings.Web;
using RazorSlices;

namespace Microsoft.AspNetCore.Http.HttpResults;

/// <summary>
/// A <see cref="RazorSlice{TModel}" /> template that is also an <see cref="IResult" /> so it can be directly returned from
/// a route handler delegate. When executed it will render the template to the response.
/// </summary>
public abstract class RazorSliceHttpResult<TModel> : RazorSlice<TModel>, IResult, IStatusCodeHttpResult, IContentTypeHttpResult
{
    /// <summary>
    /// Gets the HTTP status code: <see cref="StatusCodes.Status200OK"/>
    /// </summary>
    public int? StatusCode => StatusCodes.Status200OK;

    /// <summary>
    /// Gets the content type: <c>text/html; charset=utf-8</c>
    /// </summary>
    public string ContentType => "text/html; charset=utf-8";

    /// <summary>
    /// Gets or sets the <see cref="System.Text.Encodings.Web.HtmlEncoder" /> instance to use when rendering the template. If <c>null</c> the template
    /// will use <see cref="HtmlEncoder.Default" />.
    /// </summary>
    public HtmlEncoder? HtmlEncoder { get; set; }

    /// <inheritdoc />
    Task IResult.ExecuteAsync(HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        httpContext.Response.StatusCode = StatusCodes.Status200OK;
        httpContext.Response.ContentType = ContentType;

        var renderTask = this.RenderToPipeWriterAsync(httpContext.Response.BodyWriter, HtmlEncoder);

        if (renderTask.IsCompletedSuccessfully)
        {
            return httpContext.Response.BodyWriter.FlushAsync().AsTask();
        }

        return AwaitRenderTaskAndFlushResponse(renderTask, httpContext.Response.BodyWriter);
    }

    private static async Task AwaitRenderTaskAndFlushResponse(ValueTask renderTask, PipeWriter responseBodyWriter)
    {
        await renderTask;
        await responseBodyWriter.FlushAsync();
    }
}