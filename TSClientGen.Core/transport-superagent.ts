import * as superagent from 'superagent';
import { GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
	const url = getUri(options);
	const chain = superagent[options.method](url);

	if (options.headers) {
		Object.keys(options.headers).forEach((key) => {
			chain.set(key, options.headers[key]);
		});
	}

	if (options.data != null) {
		chain.send(options.data as object);
	}

	if (options.abortSignal != null) {
		const handler = () => {
			chain.abort();
			options.abortSignal.removeEventListener('abort', handler);
		};
		options.abortSignal.addEventListener('abort', handler);
	}

	if (options.timeout != null) {
		chain.timeout(options.timeout);
	}

	if (options.onUploadProgress != null) {
		chain.on('progress', (e) => {
			const event = {
				lengthComputable: e.total != null,
				loaded: e.loaded,
				total: e.total,
			};
			options.onUploadProgress({
				event: event as ProgressEvent,
				lengthComputable: event.lengthComputable,
				loaded: event.loaded,
				total: event.total,
			});
		});
	}

	return new Promise((resolve, reject) => chain.then((response) => resolve(response.body), reject));
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
