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
	if (request.requestBody) {
		chain = chain.send(request.requestBody);
	}
	if (request.onUploadProgress) {
		chain = chain.on('progress', request.onUploadProgress);
	}
	return new Promise((resolve, reject) => chain.then(
		response => resolve(response.body),
		reject));
}

export function getUri(options: GetUriOptions): string {
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