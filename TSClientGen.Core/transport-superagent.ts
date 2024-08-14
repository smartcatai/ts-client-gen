import { GetUriOptions, RequestOptions } from './transport-contracts';
import * as superagent from 'superagent';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	if (request.getAbortFunc != null) {
		throw new Error('SuperAgent does not support aborting http requests');
	}

	if (request.timeout != null) {
		throw new Error('Fetch API does not support timeout at the moment');
	}

	let chain: superagent.SuperAgentRequest;
	const url = getUri(request);
	switch (request.method) {
		case 'get':
			chain = superagent.get(url);
			break;
		case 'post':
			chain = superagent.post(url);
			break;
		case 'put':
			chain = superagent.put(url);
			break;
		case 'delete':
			chain = superagent.delete(url);
			break;
		case 'patch':
			chain = superagent.patch(url);
			break;
		default:
			throw new Error(`Method ${request.method} not supported`);
	}
	if (request.headers) {
		Object.keys(request.headers).forEach((key) => {
			chain.set(key, request.headers[key]);
		});
	}
	if (request.requestBody) {
		chain = chain.send(request.requestBody as object);
	}
	if (request.onUploadProgress) {
		chain = chain.on('progress', (event) => {
			request.onUploadProgress({
				event,
				lengthComputable: event.lengthComputable,
				loaded: event.loaded,
				total: event.total,
			});
		});
	}
	return new Promise((resolve, reject) => chain.then((response) => resolve(response.body), reject));
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
