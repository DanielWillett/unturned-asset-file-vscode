
import { getAssetPropertiesViewProvider } from '../extension';


export async function refreshAssetProperties()
{
    await getAssetPropertiesViewProvider().refresh();
}