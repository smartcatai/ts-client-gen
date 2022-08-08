import { RequestOptions, GetUriOptions } from './transport-contracts';
import axios, { AxiosInstance, AxiosRequestConfig } from 'axios';

axios.defaults.headers.post['Content-Type'] = 'application/json';
axios.defaults.headers.put['Content-Type'] = 'application/json';

export type AxiosInstanceTransformer = (axiosInstance: AxiosInstance) => void;

const axiosInstanceTransformers: AxiosInstanceTransformer[] = [];

export function useAxiosInstanceTransformer(axiosInstanceTransformer: AxiosInstanceTransformer) {
	axiosInstanceTransformers.push(axiosInstanceTransformer);
}

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	const options: AxiosRequestConfig = {
		url: request.url,
		method: request.method,
		params: request.queryStringParams,
		data: request.requestBody,
		onUploadProgress: request.onUploadProgress,
		timeout: request.timeout
	};
	if (typeof request.getAbortFunc == 'function') {
		options.cancelToken = new axios.CancelToken(request.getAbortFunc);
	}
	const axiosInstance = axios.create(options);
	for (let i = 0; i < axiosInstanceTransformers.length; i++) {
		axiosInstanceTransformers[i](axiosInstance);
	}
	const response = await axiosInstance.request<TResponse>(options);
	return response.data;
}

export function getUri(options: GetUriOptions): string {
	return axios.getUri({
		url: options.url,
		params: options.queryStringParams,
	});
}