import { RequestOptions, GetUriOptions } from './transport-contracts';
import axios from 'axios';

export async function request<TResponse>(request: RequestOptions): Promise<TResponse> {
	const response = await axios.request<TResponse>({
		url: request.url,
		method: request.method,
		params: request.queryStringParams,
		data: request.requestBody,
		cancelToken: request.cancelToken,
		onUploadProgress: request.onUploadProgress
	});
	return response.data;
}

export const getUri: (options: GetUriOptions) => string = axios.getUri;