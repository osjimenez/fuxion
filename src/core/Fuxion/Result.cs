using System;
using System.Collections.Generic;
using System.Text;

namespace Fuxion;

public class Result
{
	public bool IsSuccess { get; init; }
	public static Result Success() => new Result { IsSuccess = true };
}

public class Result<TResult> : Result
{
	public static Result<TResult> Success(TResult result) => new Result<TResult> { IsSuccess = true };
}

public class a
{
	public Result<string> b()
	{
		return Result<string>.Success("");
	}
}
