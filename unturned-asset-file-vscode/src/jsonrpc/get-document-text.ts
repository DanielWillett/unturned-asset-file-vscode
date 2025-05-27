import { workspace, Uri } from 'vscode';
import { RequestType } from 'vscode-languageclient';

export const GetDocumentText = new RequestType<GetDocumentTextParams, GetDocumentTextResponse, void>("unturnedDataFile/getDocumentContent");

export interface GetDocumentTextParams
{
    readonly document: string;
}
export interface GetDocumentTextResponse
{
    readonly text: string | undefined;
}

export async function handleGetDocumentText(e: GetDocumentTextParams): Promise<GetDocumentTextResponse>
{
    try
    {
        const bytes = await workspace.fs.readFile(Uri.file(e.document));

        const decoder = new TextDecoder();

        return { text: decoder.decode(bytes) };
    }
    catch (e)
    {
        console.error(e);
        return { text: undefined };
    }
}