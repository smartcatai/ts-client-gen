import { RequestOptions, GetUriOptions } from './transport-contracts';
import axios, { AxiosRequestConfig } from 'axios';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	const options: AxiosRequestConfig = {
		url: request.url,
		method: request.method,
		params: request.queryStringParams,
		data: request.requestBody,
		onUploadProgress: request.onUploadProgress
	};
	if (typeof request.getAbortFunc == 'function') {
		options.cancelToken = new axios.CancelToken(request.getAbortFunc);
	}
	const response = await axios.request<TResponse>(options);
	return response.data;
}

export const getUri: (options: GetUriOptions) => string = axios.getUri;