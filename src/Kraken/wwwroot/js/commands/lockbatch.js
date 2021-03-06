﻿define(['cmdr', 'services/releaseBatches'], function (cmdr, releaseBatchesService) {

    return new cmdr.Definition({
        name: 'LOCKBATCH',
        description: 'Locks a release batch. If a batch is locked, no modifications can be made.',
        usage: 'LOCKBATCH idOrName [comment]',
        main: function (idOrName, comment) {
            if (!idOrName) {
                this.shell.writeLine('Project batch id or name required', 'error');
                return;
            }

            return releaseBatchesService.lockReleaseBatch(idOrName, comment).then(function (data) {
                this.shell.writeLine('Batch has been locked.', 'success');
            }.bind(this)).fail(this.fail.bind(this));
        },
        autocompleteKeys: ['releaseBatches']
    });
});