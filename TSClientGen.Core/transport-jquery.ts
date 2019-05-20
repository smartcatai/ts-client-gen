import { GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	if (request.getAbortFunc != null) {
		throw new Error('JQuery does not support aborting http requests');
	}
	
	return new Promise((resolve, reject) => {
		const options: any = {
			url: getUri(request),
			method: request.method,
			parseResponseAsJson: request.jsonResponseExpected,
			success: function(data: TResponse) { resolve(data); },
			error: function(jqXhr: JQueryXHR) { reject(jqXhr); }
		};
		
		if (request.requestBody instanceof FormData) {
			options.contentType = false;
			options.processData = false;
			options.data = request.requestBody;
			if (options.onUploadProgress) {
				options.xhr = function () {
					const xhr = new XMLHttpRequest();
					xhr.upload.onprogress = options.onUploadProgress;
					return xhr;
				};
			}
		} else if (request.requestBody) {
			options.contentType = 'application/json';
			options.data = JSON.stringify(request.requestBody)
		}
		
		$.ajax(options);
	});
}

export function getUri(options: GetUriOptions): string {
	let url = options.url;
	if (options.queryStringParams) {
		const queryString = $.param(options.queryStringParams);
		if (queryString) {
			url = url + '?' + queryString;
		}
	}
	return url;
}