import { GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	if (request.getAbortFunc != null) {
		throw new Error('Fetch API does not support aborting http requests at the moment');
	}

	if (request.onUploadProgress != null) {
		throw new Error('Fetch API does not support upload progress notifications at the moment');
	}

	const fetchRequest: Partial<Request> = {
		url: getUri(request),
		method: request.method,
		body: request.requestBody,
		credentials: 'include'
	};
	return fetch(fetchRequest as Request)
		.then(response => {
			if (response.ok) {
				return request.jsonResponseExpected ? response.json() : null;
			}
			throw new Error(`Network response was not ok. Status - ${response.status}, status text - ${response.statusText}`);
		});
}

export function getUri(options: GetUriOptions) {
	const params = options.queryStringParams;
	if (!params)
		return options.url;

	const parts = Object.keys(params)
		.filter((key) => params[key] != null)
		.map((key) => {
			const value = typeof params[key] == 'object'
				? JSON.stringify(params[key])
				: params[key];
			return encodeURIComponent(key) + '=' + encodeURIComponent(value);
		});

	return options.url + (options.url.indexOf('?') === -1 ? '?' : '&') + parts.join('&');
}
