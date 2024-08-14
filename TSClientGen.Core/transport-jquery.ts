import { GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	if (request.getAbortFunc != null) {
		throw new Error('JQuery does not support aborting http requests');
	}

	if (request.timeout != null) {
		throw new Error('Fetch API does not support timeout at the moment');
	}

	return new Promise((resolve, reject) => {
		const options: any = {
			url: getUri(request),
			method: request.method,
			headers: request.headers,
			parseResponseAsJson: request.jsonResponseExpected,
			success(data: TResponse) {
				resolve(data);
			},
			error(jqXhr: JQueryXHR) {
				reject(jqXhr);
			},
		};

		if (request.requestBody instanceof FormData) {
			options.contentType = false;
			options.processData = false;
			options.data = request.requestBody;
			if (request.onUploadProgress) {
				options.xhr = () => {
					const xhr = new XMLHttpRequest();
					xhr.upload.addEventListener('progress', (event) => {
						request.onUploadProgress({
							event,
							lengthComputable: event.lengthComputable,
							loaded: event.loaded,
							total: event.total,
						});
					});
					return xhr;
				};
			}
		} else if (request.requestBody) {
			options.contentType = 'application/json';
			options.data = JSON.stringify(request.requestBody);
		}

		$.ajax(options);
	});
}

export function getUri({ baseURL, url, queryStringParams }: GetUriOptions): string {
	const urlSearchParams = new URLSearchParams();
	Object.keys(queryStringParams ?? {}).forEach((key) => {
		if (queryStringParams[key] != null) {
			urlSearchParams.set(key, queryStringParams[key].toString());
		}
	});
	return `${baseURL.replace(/[\/]+$/, '')}/${url}${urlSearchParams.size ? '?' : ''}${urlSearchParams}`;
}
