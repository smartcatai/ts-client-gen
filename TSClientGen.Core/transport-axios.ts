import { RequestOptions, GetUriOptions } from './transport-contracts';
import axios, { AxiosRequestConfig } from 'axios';

axios.defaults.headers.post['Content-Type'] = 'application/json';
axios.defaults.headers.put['Content-Type'] = 'application/json';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	const options: AxiosRequestConfig = {
		url: request.url,
		method: request.method,
		params: request.queryStringParams,
		data: request.requestBody,
		onUploadProgress: request.onUploadProgress,
		timeout: request.timeout,
		headers: request.headers,
	};
	if (typeof request.getAbortFunc == 'function') {
		options.cancelToken = new axios.CancelToken(request.getAbortFunc);
	}
	const response = await axios.request<TResponse>(options);
	return response.data;
}

export const getUri: (options: GetUriOptions) => string = function (options: GetUriOptions) {
	const axiosOptions: AxiosRequestConfig = {
		url: options.url,
		params: options.queryStringParams,
	};

	return axios.getUri(axiosOptions);
};