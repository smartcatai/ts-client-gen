import { RequestOptions, GetUriOptions } from './transport-contracts';
import axios from 'axios';

function getCancelToken(request: RequestOptions) {
	return typeof request.getAbortFunc === 'function' ? new axios.CancelToken(request.getAbortFunc) : undefined;
}

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	const response = await axios.request<TResponse>({
		baseURL: request.baseURL,
		url: request.url,
		method: request.method,
		headers: request.headers,
		params: request.queryStringParams,
		data: request.requestBody,
		onUploadProgress: request.onUploadProgress,
		cancelToken: getCancelToken(request),
		timeout: request.timeout,
	});
	return response.data;
}

export function getUri(options: GetUriOptions): string {
	return axios.getUri({
		baseURL: options.baseURL,
		url: options.url,
		params: options.queryStringParams,
	});
}
