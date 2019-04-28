import axios, { AxiosRequestConfig, CancelToken } from 'axios';

export interface NamedBlob {
	name: string,
	blob: Blob
}

export interface HttpRequestOptions {
	cancelToken?: CancelToken
}

export interface UploadFileHttpRequestOptions extends HttpRequestOptions {
	onUploadProgress?: (progressEvent: any) => void
}

export async function request<T>(config: AxiosRequestConfig) {
	const response = await axios.request<T>(config);
	return response.data;
}

export const getUri = axios.getUri;