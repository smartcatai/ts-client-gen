using System;

namespace TSClientGen
{
	/// <summary>
	/// Для размеченного данным атрибутом метода в typescript клиенте будет сгенерирован callback - параметр 
	/// для отслеживания события прогресса загрузки запроса на сервер
	/// </summary>
	[AttributeUsage(AttributeTargets.Method)]
	public class TypeScriptUploadProgressEventHandlerAttribute: Attribute
	{
	}
}