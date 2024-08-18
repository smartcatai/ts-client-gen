import { getRequestHeaders, GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
	if (options.timeout != null) {
		throw new Error('Fetch API does not support timeout at the moment');
	}

	if (options.onUploadProgress != null) {
		throw new Error('Fetch API does not support upload progress notifications at the moment');
	}

	const response = await fetch({
		url: getUri(options),
		method: options.method,
		headers: getRequestHeaders(options),
		body: options.data,
		signal: options.abortSignal,
		credentials: 'include',
	} as any);

	return +response.headers.get('Content-Length') > 0 ? response.json() : undefined;
}

export function getUri(options: GetUriOptions): string {
	const urlSearchParams = new URLSearchParams();
	Object.keys(options.params ?? {}).forEach((key) => {
		if (options.params[key] != null) {
			urlSearchParams.set(key, options.params[key].toString());
		}
	});
	return `${options.baseURL.replace(/\/+$/, '')}/${options.url}${urlSearchParams.size ? '?' : ''}${urlSearchParams}`;
}
