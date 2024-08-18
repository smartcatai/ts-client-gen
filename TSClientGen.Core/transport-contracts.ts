export interface NamedBlob {
	name: string;
	blob: Blob;
}

export interface UploadProgressEvent {
	event?: ProgressEvent;
	lengthComputable: boolean;
	loaded: number;
	total?: number;
}

export interface HttpRequestOptions {
	abortSignal?: AbortSignal;
}

export interface UploadFileHttpRequestOptions {
	abortSignal?: AbortSignal;
	timeout?: number;
	onUploadProgress?: (progressEvent: UploadProgressEvent) => void;
}

export type Method = 'get' | 'delete' | 'post' | 'put' | 'patch';

export interface RequestOptions {
	baseURL: string;
	url: string;
	method: Method;
	headers: Record<string, string>;
	data?: unknown;
	params?: Record<string, unknown>;
	abortSignal?: AbortSignal;
	timeout?: number;
	onUploadProgress?: (progressEvent: UploadProgressEvent) => void;
}

export interface GetUriOptions {
	baseURL: string;
	url: string;
	params?: Record<string, unknown>;
}

export function getRequestHeaders(options: RequestOptions): Record<string, string> {
	const headers = { ...options.headers };
	if (options.method === 'post' || options.method === 'put') {
		headers['Content-Type'] = options.data instanceof FormData ? 'multipart/form-data' : 'application/json';
	}
	return headers;
}
