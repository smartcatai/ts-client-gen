import { GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	if (request.getAbortFunc != null) {
		throw new Error('Fetch API does not support aborting http requests at the moment');
	}

	if (request.onUploadProgress != null) {
		throw new Error('Fetch API does not support upload progress notifications at the moment');
	}

	if (request.timeout != null) {
		throw new Error('Fetch API does not support timeout at the moment');
	}

	const fetchRequest: any = {
		url: getUri(request),
		method: request.method,
		headers: request.headers,
		body: request.requestBody,
		credentials: 'include',
	};

	return fetch(fetchRequest).then((response) => {
		if (response.ok) {
			return request.jsonResponseExpected ? response.json() : null;
		}
		throw new Error(`Network response was not ok. Status - ${response.status}, status text - ${response.statusText}`);
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
