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
	getAbortFunc?: (abort: () => void) => void;
}

export interface UploadFileHttpRequestOptions extends HttpRequestOptions {
	onUploadProgress?: (progressEvent: UploadProgressEvent) => void;
	timeout?: number;
}

export type Method = 'get' | 'delete' | 'post' | 'put' | 'patch';

export interface RequestOptions {
	baseURL: string;
	url: string;
	method: Method;
	headers: Record<string, string>;
	requestBody?: unknown;
	queryStringParams?: Record<string, unknown>;
	getAbortFunc?: (abort: () => void) => void;
	onUploadProgress?: (progressEvent: UploadProgressEvent) => void;
	jsonResponseExpected: boolean;
	timeout?: number;
}

export interface GetUriOptions {
	baseURL: string;
	url: string;
	queryStringParams?: Record<string, unknown>;
}
