import React, { useState } from 'react';
import { useShares, useAddShare, useRemoveShare } from '../hooks/useShares';

interface Props {
  resourceType: 'report' | 'dashboard';
  resourceId: string;
  isOpen: boolean;
  onClose: () => void;
}

export const ShareModal: React.FC<Props> = ({ resourceType, resourceId, isOpen, onClose }) => {
  const { data: shares, isLoading } = useShares(resourceType, resourceId);
  const addShare = useAddShare();
  const removeShare = useRemoveShare();
  const [userId, setUserId] = useState('');

  if (!isOpen) return null;

  const handleAdd = () => {
    if (!userId) return;
    addShare.mutate({
      resourceType,
      resourceId,
      sharedWithId: userId, // In real app, this would be selected from a user picker
      sharedWithType: 'user',
      permissionLevel: 'view'
    });
    setUserId('');
  };

  return (
    <div className="fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50">
      <div className="bg-white p-6 rounded-lg w-96 max-w-full shadow-xl">
        <div className="flex justify-between items-center mb-4">
            <h2 className="text-xl font-bold text-gray-800">Share Access</h2>
            <button onClick={onClose} className="text-gray-500 hover:text-gray-700">✕</button>
        </div>
        
        <div className="flex gap-2 mb-4">
            <input 
                value={userId}
                onChange={e => setUserId(e.target.value)}
                placeholder="User ID (UUID)"
                className="border p-2 flex-1 rounded text-gray-800"
            />
            <button 
                onClick={handleAdd} 
                disabled={addShare.isPending || !userId}
                className="bg-blue-600 text-white px-4 rounded hover:bg-blue-700 disabled:opacity-50"
            >
                Add
            </button>
        </div>

        <div className="space-y-2 max-h-60 overflow-y-auto">
            {isLoading && <div className="text-gray-500 text-center">Loading shares...</div>}
            {shares?.map(share => (
                <div key={share.id} className="flex justify-between items-center bg-gray-50 p-2 rounded border border-gray-100">
                    <span className="text-sm truncate w-48 text-gray-700" title={share.sharedWithId}>
                        {share.sharedWithId}
                    </span>
                    <button 
                        onClick={() => removeShare.mutate(share.id)} 
                        className="text-red-600 text-sm hover:underline"
                    >
                        Remove
                    </button>
                </div>
            ))}
            {!isLoading && shares?.length === 0 && (
                <div className="text-gray-500 text-sm text-center py-4 bg-gray-50 rounded border border-dashed">
                    Not shared with anyone.
                </div>
            )}
        </div>
      </div>
    </div>
  );
};





