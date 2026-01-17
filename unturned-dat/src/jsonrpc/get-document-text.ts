import { workspace, Uri } from 'vscode';
import { integer, RequestType } from 'vscode-languageclient';

export const GetDocumentText = new RequestType<GetDocumentTextParams, GetDocumentTextResponse, void>("unturnedDataFile/getDocumentContent");

export interface GetDocumentTextParams
{
    readonly document: string;
}
export interface GetDocumentTextResponse
{
    readonly text: string | undefined;
    readonly version: integer | undefined;
}

export async function handleGetDocumentText(e: GetDocumentTextParams): Promise<GetDocumentTextResponse>
{
    try
    {
        const bytes = await workspace.fs.readFile(Uri.file(e.document));

        const decoder = new TextDecoder();

        return { text: decoder.decode(bytes), version: undefined };
    }
    catch (e)
    {
        console.error(e);
        return { text: undefined, version: undefined };
    }
}