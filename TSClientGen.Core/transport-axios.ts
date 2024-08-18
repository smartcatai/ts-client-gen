import axios from 'axios';
import { getRequestHeaders, GetUriOptions, RequestOptions } from './transport-contracts';

export async function request<TResponse>(options: RequestOptions): Promise<TResponse> {
	return (await axios.request<TResponse>({
		baseURL: options.baseURL,
		url: options.url,
		method: options.method,
		headers: getRequestHeaders(options),
		data: options.data ?? undefined,
		params: options.params,
		signal: options.abortSignal,
		timeout: options.timeout,
		onUploadProgress: options.onUploadProgress,
	})).data;
}

export function getUri(options: GetUriOptions): string {
	return axios.getUri({
		baseURL: options.baseURL,
		url: options.url,
		params: options.params,
	});
}
