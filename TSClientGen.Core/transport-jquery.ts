import $ from 'jquery';
import { getRequestHeaders, GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
	return new Promise<TResponse>((resolve, reject) => {
		const ajaxOptions: JQueryAjaxSettings = {
			url: getUri(options),
			method: options.method,
			headers: getRequestHeaders(options),
			data:
				options.data instanceof FormData
					? options.data
					: options.data != null
						? JSON.stringify(options.data)
						: undefined,
			timeout: options.timeout,
			processData: false,
			success(data: TResponse) {
				resolve(data || undefined);
			},
			error(jqXhr: $.JQueryXHR) {
				reject(jqXhr);
			},
		};

		if (options.abortSignal != null) {
			ajaxOptions.beforeSend = (jqXhr: JQueryXHR) => {
				const handler = () => {
					jqXhr.abort(options.abortSignal.reason);
					options.abortSignal.removeEventListener('abort', handler);
				};
				options.abortSignal.addEventListener('abort', handler);
			};
		}

		if (options.onUploadProgress != null) {
			ajaxOptions.xhr = () => {
				const xhr = new XMLHttpRequest();
				xhr.upload.addEventListener('progress', (event) => {
					options.onUploadProgress({
						event,
						lengthComputable: event.lengthComputable,
						loaded: event.loaded,
						total: event.total,
					});
				});
				return xhr;
			};
		}

		$.ajax(ajaxOptions);
	});
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
