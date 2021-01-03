# MediatR.Pipeline.Diagnostics
Behavior for MediatR that writes basic diagnostic information to a `DiagnosticSource`.
Works best with [ElasticApm.MediatR](https://github.com/f1ana/ElasticApm.MediatR)!

**What does this do?**

This library utilizes `System.Diagnostics.DiagnosticSource` to report some common events within MediatR.  These currently include request started, request completed, and request error.

**Why would I use this?**

If you rely on Elastic APM and utilize MediatR for queries and commands, this library will help you gain visibility to when your handlers are running, and for how long.

**How to use**

This is not meant to be used directly, unless you intend on writing your own instrumentation wrapper.  Please see [ElasticApm.MediatR](https://github.com/f1ana/ElasticApm.MediatR) for more details.

**Contributions**

I welcome any and all suggestions or improvements to the codebase.  Thanks for dropping by and hope you find a good use for this library!
